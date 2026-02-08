using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;
using JobOutreach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobOutreach.Infrastructure.Repositories;

public class EmailRepository(ApplicationDbContext context) : IEmailRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Email?> GetByIdAsync(int id)
    {
        return await _context.Email.FindAsync(id);
    }

    public async Task<IEnumerable<Email>> GetAllAsync()
    {
        return await _context.Email.ToListAsync();
    }

    public async Task<Email> AddAsync(Email email)
    {
        _context.Email.Add(email);
        await _context.SaveChangesAsync();
        return email;
    }

    public async Task UpdateAsync(Email email)
    {
        _context.Email.Update(email);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var email = await _context.Email.FindAsync(id);
        if (email != null)
        {
            _context.Email.Remove(email);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Email>> GetByCountryIdAsync(int countryId, int? positionId = null)
    {
        var query = _context.Email.Where(e => e.CountryId == countryId);

        if (positionId.HasValue)
        {
            query = query.Where(e => e.PositionId == positionId.Value);
        }

        return await query.ToListAsync();
    }
}
