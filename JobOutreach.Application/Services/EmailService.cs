using JobOutreach.Application.DTO;
using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace JobOutreach.Application.Services;

public class EmailService(IEmailRepository emailRepository, ICompanyRepository companyRepository, IEmailSender emailSender, ICountryRepository countryRepository, IEmailSentRepository emailSentRepository, IConfiguration configuration) : IEmailService
{
    private readonly IEmailRepository _emailRepository = emailRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ICountryRepository _countryRepository = countryRepository;
    private readonly IEmailSentRepository _emailSentRepository = emailSentRepository;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Email?> GetEmailByIdAsync(int id)
    {
        return await _emailRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Email>> GetAllEmailsAsync()
    {
        return await _emailRepository.GetAllAsync();
    }

    public async Task<Email> CreateEmailAsync(Email email)
    {
        _ = await _countryRepository.GetByIdAsync(email.CountryId) ?? throw new ArgumentException($"Country with Id {email.CountryId} does not exist.");
        return await _emailRepository.AddAsync(email);
    }

    public async Task UpdateEmailAsync(int id, Email email)
    {
        var existingEmail = await _emailRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Email with Id {id} not found.");
        _ = await _countryRepository.GetByIdAsync(email.CountryId) ?? throw new ArgumentException($"Country with Id {email.CountryId} does not exist.");
        existingEmail.Subject = email.Subject;
        existingEmail.Content = email.Content;
        existingEmail.Attachment = email.Attachment;
        existingEmail.SentBy = email.SentBy;
        existingEmail.CountryId = email.CountryId;

        await _emailRepository.UpdateAsync(existingEmail);
    }

    public async Task DeleteEmailAsync(int id)
    {
        _ = await _emailRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Email with Id {id} not found.");

        await _emailRepository.DeleteAsync(id);
    }

    public async Task SendEmailsAsync(SendEmailRequestDto request)
    {
        var emailTemplate = await _emailRepository.GetByIdAsync(request.EmailTemplateId)
             ?? throw new KeyNotFoundException($"Email Template with Id {request.EmailTemplateId} not found.");

        var configuredEmail = _configuration["Smtp:Username"];
        if (emailTemplate.SentBy != configuredEmail)
        {
            throw new InvalidOperationException($"Email can only be sent from '{configuredEmail}'. Please update the Email Template.");
        }

        var companies = await _companyRepository.GetByIdsAsync(request.CompanyIds);

        string? attachmentPath = null;
        if (!string.IsNullOrEmpty(emailTemplate.Attachment))
        {
            attachmentPath = Path.Combine(AppContext.BaseDirectory, emailTemplate.Attachment);

            if (!File.Exists(attachmentPath))
            {
                 var currentDir = Directory.GetCurrentDirectory();
                 var altPath = Path.Combine(currentDir, emailTemplate.Attachment);
                 if (File.Exists(altPath))
                 {
                     attachmentPath = altPath;
                 }
            }
        }

        foreach (var company in companies)
        {
            if (!string.IsNullOrEmpty(company.CompanyEmail))
            {
                await _emailSender.SendEmailAsync(
                    to: company.CompanyEmail,
                    subject: emailTemplate.Subject,
                    content: emailTemplate.Content,
                    from: emailTemplate.SentBy,
                    attachmentPath: attachmentPath
                );

                // Add to EmailSent
                if (emailTemplate.PositionId.HasValue)
                {
                    var emailSent = new EmailSent
                    {
                        EmailId = emailTemplate.Id,
                        CompanyId = company.Id,
                        PositionId = emailTemplate.PositionId.Value,
                        SentToEmail = company.CompanyEmail,
                        SentAt = DateTime.UtcNow
                    };
                    await _emailSentRepository.AddAsync(emailSent);
                }

                if (!company.IsContacted)
                {
                    company.IsContacted = true;
                    await _companyRepository.UpdateAsync(company);
                }
            }
        }
    }

    public async Task<IEnumerable<Email>> GetEmailsByCountryIdAsync(int countryId, int? positionId = null)
    {
        _ = await _countryRepository.GetByIdAsync(countryId) ?? throw new KeyNotFoundException($"Country with ID {countryId} not found.");
        return await _emailRepository.GetByCountryIdAsync(countryId, positionId);
    }
}
