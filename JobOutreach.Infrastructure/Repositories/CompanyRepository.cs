using System.Linq.Expressions;
using JobOutreach.Application.Common;
using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;
using JobOutreach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobOutreach.Infrastructure.Repositories;

public class CompanyRepository(
    ApplicationDbContext context
) : ICompanyRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PagedResult<Company>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm = null,
    string? sortColumn = null,
    string? sortOrder = null)
    {
        var query = _context.Company
            .Include(c => c.Positions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.CompanyName.Contains(searchTerm) ||
                c.CompanyEmail.Contains(searchTerm) ||
                c.CompanyPhone.Contains(searchTerm) ||
                c.CompanyUrl.Contains(searchTerm) ||
                c.Positions.Any(p => p.PositionName.Contains(searchTerm))
            );
        }

        query = ApplySorting(query, sortColumn, sortOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Company>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Company>> GetUncontactedPagedAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm = null,
    string? sortColumn = null,
    string? sortOrder = null)
    {
        var query = _context.Company
            .Include(c => c.Positions)
            .Where(c => !c.IsContacted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.CompanyName.Contains(searchTerm) ||
                c.CompanyEmail.Contains(searchTerm) ||
                c.CompanyPhone.Contains(searchTerm) ||
                c.CompanyUrl.Contains(searchTerm) ||
                c.Positions.Any(p => p.PositionName.Contains(searchTerm))
            );
        }

        query = ApplySorting(query, sortColumn, sortOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Company>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static IQueryable<Company> ApplySorting(
    IQueryable<Company> query,
    string? sortColumn,
    string? sortOrder)
    {
        bool isAscending = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return query.OrderByDescending(c => c.Id);
        }

        return sortColumn.ToLower() switch
        {
            "companyname" => isAscending ? query.OrderBy(c => c.CompanyName) : query.OrderByDescending(c => c.CompanyName),
            "companyemail" => isAscending ? query.OrderBy(c => c.CompanyEmail) : query.OrderByDescending(c => c.CompanyEmail),
            "companyphone" => isAscending ? query.OrderBy(c => c.CompanyPhone) : query.OrderByDescending(c => c.CompanyPhone),
            "companyurl" => isAscending ? query.OrderBy(c => c.CompanyUrl) : query.OrderByDescending(c => c.CompanyUrl),
            "id" => isAscending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id),
            _ => query.OrderByDescending(c => c.Id)
        };
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _context.Company
            .Include(c => c.Positions)
            .ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _context.Company
            .Include(c => c.Positions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Company
            .Where(c => ids.Contains(c.Id))
            .Include(c => c.Positions)
            .ToListAsync();
    }

    public async Task<Company> AddAsync(Company company)
    {
        _context.Company.Add(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        _context.Entry(company).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var company = await _context.Company.FindAsync(id);
        if (company != null)
        {
            _context.Company.Remove(company);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddPositionToCompanyAsync(int companyId, int positionId)
    {
        var companyExists = await _context.Company.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new KeyNotFoundException($"Company with Id {companyId} not found.");
        }

        var positionExists = await _context.Position.AnyAsync(p => p.Id == positionId);
        if (!positionExists)
        {
            throw new KeyNotFoundException($"Position with Id {positionId} not found.");
        }

        var alreadyExists = await _context.CompanyPosition
            .AnyAsync(cp => cp.CompanyId == companyId && cp.PositionId == positionId);

        if (!alreadyExists)
        {
            _context.CompanyPosition.Add(new CompanyPosition
            {
                CompanyId = companyId,
                PositionId = positionId
            });

            await _context.SaveChangesAsync();
        }
    }

    public async Task AddPositionsToCompanyAsync(int companyId, IEnumerable<int> positionIds)
    {
        var ids = positionIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
        {
            throw new ArgumentException("At least one positionId is required.", nameof(positionIds));
        }

        var companyExists = await _context.Company.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new KeyNotFoundException($"Company with Id {companyId} not found.");
        }

        var existingPositionIds = await _context.Position
            .Where(p => ids.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        if (existingPositionIds.Count != ids.Count)
        {
            var missingIds = ids.Except(existingPositionIds);
            throw new KeyNotFoundException($"One or more positions not found. Missing Ids: {string.Join(", ", missingIds)}");
        }

        var existingCompanyPositionIds = await _context.CompanyPosition
            .Where(cp => cp.CompanyId == companyId && ids.Contains(cp.PositionId))
            .Select(cp => cp.PositionId)
            .ToListAsync();

        var newIds = ids.Except(existingCompanyPositionIds).ToList();

        if (newIds.Count == 0)
        {
            return;
        }

        foreach (var positionId in newIds)
        {
            _context.CompanyPosition.Add(new CompanyPosition
            {
                CompanyId = companyId,
                PositionId = positionId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateCompanyPositionsAsync(int companyId, IEnumerable<int> positionIds)
    {
        var ids = positionIds?.Distinct().ToList() ?? [];

        var companyExists = await _context.Company.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new KeyNotFoundException($"Company with Id {companyId} not found.");
        }

        if (ids.Count > 0)
        {
            var existingPositionIds = await _context.Position
                .Where(p => ids.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (existingPositionIds.Count != ids.Count)
            {
                var missingIds = ids.Except(existingPositionIds);
                throw new KeyNotFoundException($"One or more positions not found. Missing Ids: {string.Join(", ", missingIds)}");
            }
        }

        var existingCompanyPositions = await _context.CompanyPosition
            .Where(cp => cp.CompanyId == companyId)
            .ToListAsync();

        var existingCompanyPositionIds = existingCompanyPositions
            .Select(cp => cp.PositionId)
            .ToList();

        var toAdd = ids.Except(existingCompanyPositionIds).ToList();
        var toRemove = existingCompanyPositionIds.Except(ids).ToList();

        if (toRemove.Count > 0)
        {
            var removeEntities = existingCompanyPositions
                .Where(cp => toRemove.Contains(cp.PositionId))
                .ToList();

            _context.CompanyPosition.RemoveRange(removeEntities);
        }

        foreach (var positionId in toAdd)
        {
            _context.CompanyPosition.Add(new CompanyPosition
            {
                CompanyId = companyId,
                PositionId = positionId
            });
        }

        await _context.SaveChangesAsync();
    }
}
