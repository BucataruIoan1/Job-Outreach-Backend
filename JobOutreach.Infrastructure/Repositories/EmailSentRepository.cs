using JobOutreach.Application.Common;
using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;
using JobOutreach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobOutreach.Infrastructure.Repositories;

public class EmailSentRepository(ApplicationDbContext context) : IEmailSentRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(EmailSent emailSent)
    {
        _context.EmailSent.Add(emailSent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var emailSent = await _context.EmailSent.FindAsync(id);
        if (emailSent != null)
        {
            _context.EmailSent.Remove(emailSent);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<EmailSent>> GetAllAsync()
    {
        return await _context.EmailSent
            .Include(es => es.Email)
            .Include(es => es.Company)
            .Include(es => es.Position)
            .OrderByDescending(es => es.SentAt)
            .ToListAsync();
    }

    public async Task<PagedResult<EmailSent>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.EmailSent
            .Include(es => es.Email)
            .Include(es => es.Company)
            .Include(es => es.Position)
            .OrderByDescending(es => es.SentAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<EmailSent>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
