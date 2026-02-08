using JobOutreach.Application.DTO;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public interface IEmailService
{
    Task<Email?> GetEmailByIdAsync(int id);
    Task<IEnumerable<Email>> GetAllEmailsAsync();
    Task<Email> CreateEmailAsync(Email email);
    Task UpdateEmailAsync(int id, Email email);
    Task DeleteEmailAsync(int id);
    Task SendEmailsAsync(SendEmailRequestDto request);
    Task<IEnumerable<Email>> GetEmailsByCountryIdAsync(int countryId, int? positionId = null);
}
