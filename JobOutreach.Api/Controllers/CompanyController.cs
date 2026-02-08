using JobOutreach.Application.DTO;
using JobOutreach.Application.Services;
using JobOutreach.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobOutreach.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController(
    ICompanyService companyService
) : ControllerBase
{
    private readonly ICompanyService _companyService = companyService;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortColumn = null, [FromQuery] string? sortOrder = null)
    {
        if (id.HasValue)
        {
            var company = await _companyService.GetCompanyByIdAsync(id.Value);
            if (company == null)
            {
                return NotFound();
            }
            return Ok(company);
        }

        var validPageSizes = new[] { 5, 10, 20 };
        if (!validPageSizes.Contains(pageSize))
        {
            return BadRequest("Page size must be 5, 10, or 20.");
        }

        var result = await _companyService.GetCompaniesPagedAsync(page, pageSize, search, sortColumn, sortOrder);
        return Ok(result);
    }

    [HttpGet("uncontacted")]
    public async Task<IActionResult> GetUncontacted([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortColumn = null, [FromQuery] string? sortOrder = null)
    {
        var validPageSizes = new[] { 5, 10, 20 };
        if (!validPageSizes.Contains(pageSize))
        {
            return BadRequest("Page size must be 5, 10, or 20.");
        }

        var result = await _companyService.GetUncontactedCompaniesPagedAsync(page, pageSize, search, sortColumn, sortOrder);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto createCompanyDto)
    {
        if (createCompanyDto == null)
        {
            return BadRequest();
        }

        var company = new Company
        {
            CompanyName = createCompanyDto.CompanyName,
            CompanyPhone = createCompanyDto.CompanyPhone,
            CompanyEmail = createCompanyDto.CompanyEmail,
            CompanyUrl = createCompanyDto.CompanyUrl,
            IsContacted = false // Default to false
        };

        var createdCompany = await _companyService.CreateCompanyAsync(company);

        return CreatedAtAction(nameof(Get), new { id = createdCompany.Id }, createdCompany);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyDto updateCompanyDto)
    {
        if (updateCompanyDto == null)
        {
            return BadRequest();
        }

        var company = new Company
        {
            Id = id,
            CompanyName = updateCompanyDto.CompanyName,
            CompanyPhone = updateCompanyDto.CompanyPhone,
            CompanyEmail = updateCompanyDto.CompanyEmail,
            CompanyUrl = updateCompanyDto.CompanyUrl
        };

        try
        {
            if (updateCompanyDto.PositionIds is not null)
            {
                await _companyService.UpdateCompanyPositionsAsync(id, updateCompanyDto.PositionIds);
            }

            await _companyService.UpdateCompanyAsync(id, company);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _companyService.DeleteCompanyAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException != null && ex.InnerException.Message.Contains("REFERENCE constraint"))
            {
                return BadRequest("Cannot delete company because it has associated data (e.g. sent emails). Please delete the associated data first.");
            }
            return StatusCode(500, "An error occurred while deleting the company.");
        }
    }

    [HttpPost("{companyId}/positions")]
    public async Task<IActionResult> AddPositionsToCompany(int companyId, [FromQuery] List<int> positionIds)
    {
        if (positionIds == null || positionIds.Count == 0)
        {
            return BadRequest("At least one positionId must be provided.");
        }

        try
        {
            await _companyService.AddPositionsToCompanyAsync(companyId, positionIds);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
