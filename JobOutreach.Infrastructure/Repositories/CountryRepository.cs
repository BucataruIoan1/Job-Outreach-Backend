using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;
using JobOutreach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobOutreach.Infrastructure.Repositories;

public class CountryRepository(ApplicationDbContext context) : ICountryRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Country?> GetByIdAsync(int id)
    {
        return await _context.Country.FindAsync(id);
    }

    public async Task<Country> AddAsync(Country country)
    {
        _context.Country.Add(country);
        await _context.SaveChangesAsync();
        return country;
    }

    public async Task UpdateAsync(Country country)
    {
        _context.Country.Update(country);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var country = await _context.Country.FindAsync(id);
        if (country != null)
        {
            _context.Country.Remove(country);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        return await _context.Country.ToListAsync();
    }
}
