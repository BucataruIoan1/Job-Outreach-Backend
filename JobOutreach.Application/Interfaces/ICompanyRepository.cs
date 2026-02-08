using JobOutreach.Application.Common;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Interfaces;

public interface ICompanyRepository
{
    Task<PagedResult<Company>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortColumn = null, string? sortOrder = null);
    Task<PagedResult<Company>> GetUncontactedPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortColumn = null, string? sortOrder = null);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company?> GetByIdAsync(int id);
    Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Company> AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(int id);
    Task AddPositionToCompanyAsync(int companyId, int positionId);
    Task AddPositionsToCompanyAsync(int companyId, IEnumerable<int> positionIds);
    Task UpdateCompanyPositionsAsync(int companyId, IEnumerable<int> positionIds);
}
