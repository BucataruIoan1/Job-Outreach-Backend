using JobOutreach.Application.DTO;
using JobOutreach.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobOutreach.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailSentController(IEmailSentService emailSentService) : ControllerBase
{
    private readonly IEmailSentService _emailSentService = emailSentService;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        int[] validPageSizes = { 5, 10, 20 };
        if (!validPageSizes.Contains(pageSize))
        {
            return BadRequest($"Invalid page size. Allowed values are: {string.Join(", ", validPageSizes)}");
        }

        var pagedResult = await _emailSentService.GetEmailSentPagedAsync(page, pageSize);

        var dtos = pagedResult.Items.Select(es => new EmailSentDto
        {
            Id = es.Id,
            Subject = es.Email?.Subject ?? "Unknown",
            CompanyName = es.Company?.CompanyName ?? "Unknown",
            PositionName = es.Position?.PositionName ?? "Unknown",
            SentToEmail = es.SentToEmail,
            SentAt = es.SentAt
        });

        return Ok(new
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalPages = (int)Math.Ceiling((double)pagedResult.TotalCount / pagedResult.PageSize)
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _emailSentService.DeleteEmailSentAsync(id);
        return NoContent();
    }
}
