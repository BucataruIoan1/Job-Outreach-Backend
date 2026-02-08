using JobOutreach.Application.DTO;
using JobOutreach.Application.Services;
using JobOutreach.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace JobOutreach.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionController(IPositionService positionService) : ControllerBase
{
    private readonly IPositionService _positionService = positionService;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var position = await _positionService.GetPositionByIdAsync(id.Value);
            if (position == null)
            {
                return NotFound();
            }
            return Ok(position);
        }

        var positions = await _positionService.GetAllPositionsAsync();
        return Ok(positions);
    }

    [HttpGet("country/{countryId}")]
    public async Task<IActionResult> GetByCountryId(int countryId)
    {
        var positions = await _positionService.GetPositionsByCountryIdAsync(countryId);
        return Ok(positions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePositionDto createPositionDto)
    {
        if (createPositionDto == null)
        {
            return BadRequest();
        }

        var position = new Position
        {
            PositionName = createPositionDto.PositionName
        };

        var createdPosition = await _positionService.CreatePositionAsync(position);

        return CreatedAtAction(nameof(Get), new { id = createdPosition.Id }, createdPosition);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePositionDto updatePositionDto)
    {
        if (updatePositionDto == null)
        {
            return BadRequest();
        }

        var position = new Position
        {
            Id = id,
            PositionName = updatePositionDto.PositionName
        };

        try
        {
            await _positionService.UpdatePositionAsync(id, position);
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
            await _positionService.DeletePositionAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

