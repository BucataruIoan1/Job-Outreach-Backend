using JobOutreach.Application.DTO;
using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public class CountryService(ICountryRepository countryRepository) : ICountryService
{
    private readonly ICountryRepository _countryRepository = countryRepository;

    public async Task<Country> CreateCountryAsync(CreateCountryDto createCountryDto)
    {
        var country = new Country
        {
            CountryName = createCountryDto.CountryName,
            CountryCode = createCountryDto.CountryCode
        };

        return await _countryRepository.AddAsync(country);
    }

    public async Task UpdateCountryAsync(int id, UpdateCountryDto updateCountryDto)
    {
        var existingCountry = await _countryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Country with Id {id} not found.");

        existingCountry.CountryName = updateCountryDto.CountryName;
        existingCountry.CountryCode = updateCountryDto.CountryCode;

        await _countryRepository.UpdateAsync(existingCountry);
    }

    public async Task DeleteCountryAsync(int id)
    {
        _ = await _countryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Country with Id {id} not found.");

        await _countryRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Country>> GetAllCountriesAsync()
    {
        return await _countryRepository.GetAllAsync();
    }

    public async Task<Country?> GetCountryByIdAsync(int id)
    {
        return await _countryRepository.GetByIdAsync(id);
    }
}
