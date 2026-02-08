using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Interfaces;

public interface IEmailRepository
{
    Task<Email?> GetByIdAsync(int id);
    Task<IEnumerable<Email>> GetAllAsync();
    Task<Email> AddAsync(Email email);
    Task UpdateAsync(Email email);
    Task DeleteAsync(int id);
    Task<IEnumerable<Email>> GetByCountryIdAsync(int countryId, int? positionId = null);
}
