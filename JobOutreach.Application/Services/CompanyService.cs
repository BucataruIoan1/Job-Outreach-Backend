using JobOutreach.Application.Common;
using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public class CompanyService(ICompanyRepository companyRepository) : ICompanyService
{
    private readonly ICompanyRepository _companyRepository = companyRepository;

    public async Task<PagedResult<Company>> GetCompaniesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortColumn = null, string? sortOrder = null)
        => await _companyRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

    public async Task<PagedResult<Company>> GetUncontactedCompaniesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortColumn = null, string? sortOrder = null)
        => await _companyRepository.GetUncontactedPagedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

    public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        => await _companyRepository.GetAllAsync();

    public async Task<Company?> GetCompanyByIdAsync(int id)
        => await _companyRepository.GetByIdAsync(id);

    public async Task<Company> CreateCompanyAsync(Company company)
        => await _companyRepository.AddAsync(company);

    public async Task UpdateCompanyAsync(int id, Company company)
    {
        var existingCompany = await _companyRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Company with Id {id} not found.");

        existingCompany.CompanyName = company.CompanyName;
        existingCompany.CompanyPhone = company.CompanyPhone;
        existingCompany.CompanyEmail = company.CompanyEmail;
        existingCompany.CompanyUrl = company.CompanyUrl;

        await _companyRepository.UpdateAsync(existingCompany);
    }

    public async Task DeleteCompanyAsync(int id)
    {
        _ = await _companyRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Company with Id {id} not found.");

        await _companyRepository.DeleteAsync(id);
    }

    public async Task AddPositionToCompanyAsync(int companyId, int positionId)
        => await _companyRepository.AddPositionToCompanyAsync(companyId, positionId);

    public async Task AddPositionsToCompanyAsync(int companyId, IEnumerable<int> positionIds)
        => await _companyRepository.AddPositionsToCompanyAsync(companyId, positionIds);

    public async Task UpdateCompanyPositionsAsync(int companyId, IEnumerable<int> positionIds)
        => await _companyRepository.UpdateCompanyPositionsAsync(companyId, positionIds);
}
