using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;
using JobOutreach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobOutreach.Infrastructure.Repositories;

public class PositionRepository(ApplicationDbContext context) : IPositionRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Position>> GetAllAsync()
    {
        return await _context.Position.ToListAsync();
    }

    public async Task<Position?> GetByIdAsync(int id)
    {
        return await _context.Position.FindAsync(id);
    }

    public async Task<IEnumerable<Position>> GetByCountryIdAsync(int countryId)
    {
        return await _context.Email
            .Where(e => e.CountryId == countryId && e.PositionId.HasValue)
            .Select(e => e.Position!)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Position> AddAsync(Position position)
    {
        _context.Position.Add(position);
        await _context.SaveChangesAsync();
        return position;
    }

    public async Task UpdateAsync(Position position)
    {
        _context.Entry(position).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var position = await _context.Position.FindAsync(id);
        if (position != null)
        {
            _context.Position.Remove(position);
            await _context.SaveChangesAsync();
        }
    }
}

