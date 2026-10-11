using System.Net;
using JobOutreach.Application.Interfaces;
using JobOutreach.Infrastructure.BackgroundJobs;
using JobOutreach.Infrastructure.Persistence;
using JobOutreach.Infrastructure.Repositories;
using JobOutreach.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobOutreach.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IEmailSentRepository, EmailSentRepository>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddHttpClient(CompanyPageMonitorJob.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        });

        services.AddHostedService<CompanyPageMonitorJob>();

        return services;
    }
}
