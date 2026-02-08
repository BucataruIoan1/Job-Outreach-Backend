using JobOutreach.Application.DTO;
using JobOutreach.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobOutreach.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountryController(ICountryService countryService) : ControllerBase
{
    private readonly ICountryService _countryService = countryService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCountryDto createCountryDto)
    {
        if (createCountryDto == null)
        {
            return BadRequest();
        }

        var country = await _countryService.CreateCountryAsync(createCountryDto);
        return CreatedAtAction(nameof(Get), new { id = country.Id }, country);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCountryDto updateCountryDto)
    {
        if (updateCountryDto == null)
        {
            return BadRequest();
        }

        try
        {
            await _countryService.UpdateCountryAsync(id, updateCountryDto);
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
            await _countryService.DeleteCountryAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var country = await _countryService.GetCountryByIdAsync(id.Value);
            if (country == null)
            {
                return NotFound();
            }
            return Ok(country);
        }

        var countries = await _countryService.GetAllCountriesAsync();
        return Ok(countries);
    }
}
