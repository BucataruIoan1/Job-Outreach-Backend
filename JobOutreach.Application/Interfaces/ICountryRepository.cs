using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Interfaces;

public interface ICountryRepository
{
    Task<Country?> GetByIdAsync(int id);
    Task<Country> AddAsync(Country country);
    Task UpdateAsync(Country country);
    Task DeleteAsync(int id);
    Task<IEnumerable<Country>> GetAllAsync();
}
