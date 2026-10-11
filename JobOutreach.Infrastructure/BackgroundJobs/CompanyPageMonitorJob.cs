using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;
using JobOutreach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobOutreach.Infrastructure.BackgroundJobs;

public class CompanyPageMonitorJob(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<CompanyPageMonitorJob> logger
) : BackgroundService
{
    public const string HttpClientName = "CompanyPageMonitor";

    private const int MaxConcurrency = 5;
    private const int MaxErrorLength = 1000;

    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<CompanyPageMonitorJob> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        try
        {
            await RunAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Company page monitor failed.");
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var companies = await context.Company
            .AsNoTracking()
            .Where(c => c.CompanyUrl != "")
            .Select(c => new CompanyInfo(c.Id, c.CompanyName, c.CompanyUrl))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Company page monitor started for {Count} companies.", companies.Count);

        var results = new ConcurrentBag<PageResult>();
        await Parallel.ForEachAsync(
            companies,
            new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrency, CancellationToken = cancellationToken },
            async (company, token) => results.Add(await FetchAsync(company, token)));

        var snapshots = await context.CompanyPageSnapshot.ToDictionaryAsync(s => s.CompanyId, cancellationToken);
        var now = DateTime.UtcNow;
        var changes = new List<PageChange>();

        foreach (var result in results)
        {
            snapshots.TryGetValue(result.Company.Id, out var snapshot);

            if (result.Content is null)
            {
                _logger.LogWarning("Could not check {Company} ({Url}): {Error}", result.Company.Name, result.Company.Url, result.Error);

                if (snapshot is not null)
                {
                    snapshot.LastCheckedAt = now;
                    snapshot.LastStatusCode = result.StatusCode;
                    snapshot.LastError = Truncate(result.Error, MaxErrorLength);
                }
                continue;
            }

            var hash = ComputeHash(result.Content);

            if (snapshot is null)
            {
                context.CompanyPageSnapshot.Add(new CompanyPageSnapshot
                {
                    CompanyId = result.Company.Id,
                    Url = result.Company.Url,
                    ContentHash = hash,
                    Content = result.Content,
                    LastCheckedAt = now,
                    LastStatusCode = result.StatusCode
                });
                continue;
            }

            snapshot.LastCheckedAt = now;
            snapshot.LastStatusCode = result.StatusCode;
            snapshot.LastError = null;

            if (snapshot.Url != result.Company.Url)
            {
                snapshot.Url = result.Company.Url;
                snapshot.ContentHash = hash;
                snapshot.Content = result.Content;
                continue;
            }

            if (snapshot.ContentHash != hash)
            {
                changes.Add(new PageChange(snapshot, result.Company, result.Content, hash));
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        if (changes.Count == 0)
        {
            _logger.LogInformation("Company page monitor finished: no changes.");
            return;
        }

        var address = _configuration["Smtp:Username"];
        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.LogWarning("Company page monitor found {Count} changes, but Smtp:Username is not configured.", changes.Count);
            return;
        }

        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        await emailSender.SendEmailAsync(
            address,
            $"[Job Outreach] {changes.Count} pagini de cariere modificate",
            BuildEmailBody(changes),
            address,
            null);

        foreach (var change in changes)
        {
            change.Snapshot.ContentHash = change.NewHash;
            change.Snapshot.Content = change.NewContent;
            change.Snapshot.LastChangedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company page monitor finished: {Count} changes sent to {Address}.", changes.Count, address);
    }

    private async Task<PageResult> FetchAsync(CompanyInfo company, CancellationToken cancellationToken)
    {
        var uri = ToUri(company.Url);
        if (uri is null)
        {
            return new PageResult(company, null, null, "Invalid URL.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(uri, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return new PageResult(company, statusCode, null, $"HTTP {statusCode}");
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            return new PageResult(company, statusCode, ExtractText(html), null);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new PageResult(company, null, null, ex.Message);
        }
    }
    private static string ExtractText(string html)
    {
        var document = new HtmlParser().ParseDocument(html);

        foreach (var element in document.QuerySelectorAll("script, style, noscript, template, svg, iframe"))
        {
            element.Remove();
        }

        INode root = (INode?)document.Body ?? document.DocumentElement;

        var lines = root.Descendants<IText>()
            .Select(text => Whitespace.Replace(text.Data, " ").Trim())
            .Where(line => line.Length > 0);

        return string.Join('\n', lines);
    }

    private static string ComputeHash(string content) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();

    private static Uri? ToUri(string url)
    {
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && !Uri.TryCreate("https://" + url.Trim(), UriKind.Absolute, out uri))
        {
            return null;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ? uri : null;
    }

    private static string BuildEmailBody(IEnumerable<PageChange> changes)
    {
        var body = new StringBuilder();
        body.Append("<h2>Pagini de cariere modificate</h2><ul>");

        foreach (var change in changes.OrderBy(c => c.Company.Name))
        {
            body.Append($"<li><a href=\"{WebUtility.HtmlEncode(change.Company.Url)}\">{WebUtility.HtmlEncode(change.Company.Name)}</a></li>");
        }

        body.Append("</ul>");

        return body.ToString();
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];

    private sealed record CompanyInfo(int Id, string Name, string Url);

    private sealed record PageResult(CompanyInfo Company, int? StatusCode, string? Content, string? Error);

    private sealed record PageChange(CompanyPageSnapshot Snapshot, CompanyInfo Company, string NewContent, string NewHash);
}
