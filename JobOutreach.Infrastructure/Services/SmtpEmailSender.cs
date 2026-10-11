using System.Net;
using System.Net.Mail;
using JobOutreach.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace JobOutreach.Infrastructure.Services;

public class SmtpEmailSender(
    IConfiguration configuration
) : IEmailSender
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendEmailAsync(string to, string subject, string content, string from, string? attachmentPath)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:Username"];
        var smtpPass = _configuration["Smtp:Password"];

        if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            throw new InvalidOperationException("SMTP credentials are not configured.");
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = content,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
        {
            mailMessage.Attachments.Add(new Attachment(attachmentPath));
        }

        await client.SendMailAsync(mailMessage);
    }
}
