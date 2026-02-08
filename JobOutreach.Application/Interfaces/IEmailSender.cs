namespace JobOutreach.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string content, string from, string? attachmentPath);
}
