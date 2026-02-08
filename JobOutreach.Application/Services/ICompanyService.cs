using JobOutreach.Application.Common;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public interface ICompanyService
{
    Task<PagedResult<Company>> GetCompaniesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortColumn = null, string? sortOrder = null);
    Task<PagedResult<Company>> GetUncontactedCompaniesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortColumn = null, string? sortOrder = null);
    Task<IEnumerable<Company>> GetAllCompaniesAsync();
    Task<Company?> GetCompanyByIdAsync(int id);
    Task<Company> CreateCompanyAsync(Company company);
    Task UpdateCompanyAsync(int id, Company company);
    Task DeleteCompanyAsync(int id);
    Task AddPositionToCompanyAsync(int companyId, int positionId);
    Task AddPositionsToCompanyAsync(int companyId, IEnumerable<int> positionIds);
    Task UpdateCompanyPositionsAsync(int companyId, IEnumerable<int> positionIds);
}
