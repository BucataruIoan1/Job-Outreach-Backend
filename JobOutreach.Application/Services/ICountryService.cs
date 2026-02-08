using JobOutreach.Application.DTO;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public interface ICountryService
{
    Task<Country> CreateCountryAsync(CreateCountryDto createCountryDto);
    Task UpdateCountryAsync(int id, UpdateCountryDto updateCountryDto);
    Task DeleteCountryAsync(int id);
    Task<IEnumerable<Country>> GetAllCountriesAsync();
    Task<Country?> GetCountryByIdAsync(int id);
}
