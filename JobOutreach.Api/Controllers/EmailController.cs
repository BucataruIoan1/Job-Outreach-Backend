using JobOutreach.Application.DTO;
using JobOutreach.Application.Services;
using JobOutreach.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace JobOutreach.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController(
    IEmailService emailService,
    ICountryService countryService,
    IWebHostEnvironment env,
    IConfiguration configuration
) : ControllerBase
{
    private readonly IEmailService _emailService = emailService;
    private readonly ICountryService _countryService = countryService;
    private readonly IWebHostEnvironment _env = env;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var email = await _emailService.GetEmailByIdAsync(id.Value);
            if (email == null)
            {
                return NotFound();
            }
            return Ok(email);
        }

        var emails = await _emailService.GetAllEmailsAsync();
        return Ok(emails);
    }

    [HttpGet("{id}/attachment")]
    public async Task<IActionResult> GetAttachment(int id)
    {
        var email = await _emailService.GetEmailByIdAsync(id);
        if (email == null || string.IsNullOrWhiteSpace(email.Attachment))
            return NotFound();

        var fullPath = Path.Combine(_env.ContentRootPath, email.Attachment);
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var fileName = Path.GetFileName(fullPath);
        return PhysicalFile(fullPath, "application/pdf", fileName);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateEmailWithFileDto createEmailDto)
    {
        if (createEmailDto == null)
        {
            return BadRequest();
        }

        if (createEmailDto.PdfFile == null || createEmailDto.PdfFile.Length == 0)
        {
            return BadRequest("PdfFile is required.");
        }

        string attachmentPath = string.Empty;

        var country = await _countryService.GetCountryByIdAsync(createEmailDto.CountryId);
        if (country == null)
        {
            return BadRequest($"Country with Id {createEmailDto.CountryId} does not exist.");
        }

        var countryName = country.CountryName;
        
        var folderPath = Path.Combine(_env.ContentRootPath, "CV", countryName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileName = createEmailDto.PdfFile.FileName;
        var fullPath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await createEmailDto.PdfFile.CopyToAsync(stream);
        }

        attachmentPath = Path.Combine("CV", countryName, fileName);

        var email = new Email
        {
            Subject = createEmailDto.Subject,
            Content = createEmailDto.Content,
            Attachment = attachmentPath,
            SentBy = _configuration["Smtp:Username"] ?? "add your address email here",
            CountryId = createEmailDto.CountryId,
            PositionId = createEmailDto.PositionId
        };

        var createdEmail = await _emailService.CreateEmailAsync(email);
        return CreatedAtAction(nameof(Get), new { id = createdEmail.Id }, createdEmail);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _emailService.DeleteEmailAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateEmailDto updateEmailDto)
    {
        if (updateEmailDto == null)
        {
            return BadRequest();
        }

        string? attachmentPath = updateEmailDto.Attachment;

        if (updateEmailDto.PdfFile != null && updateEmailDto.PdfFile.Length > 0)
        {
             var country = await _countryService.GetCountryByIdAsync(updateEmailDto.CountryId);
             if (country == null)
             {
                 return BadRequest($"Country with Id {updateEmailDto.CountryId} does not exist.");
             }

             var countryName = country.CountryName;
             
             var folderPath = Path.Combine(_env.ContentRootPath, "CV", countryName);
             if (!Directory.Exists(folderPath))
             {
                 Directory.CreateDirectory(folderPath);
             }

             var fileName = updateEmailDto.PdfFile.FileName;
             var fullPath = Path.Combine(folderPath, fileName);

             using (var stream = new FileStream(fullPath, FileMode.Create))
             {
                 await updateEmailDto.PdfFile.CopyToAsync(stream);
             }

             attachmentPath = Path.Combine("CV", countryName, fileName);
        }

        var email = new Email
        {
            Id = id,
            Subject = updateEmailDto.Subject,
            Content = updateEmailDto.Content,
            Attachment = attachmentPath ?? string.Empty,
            SentBy = updateEmailDto.SentBy,
            CountryId = updateEmailDto.CountryId,
            PositionId = updateEmailDto.PositionId
        };

        try
        {
            await _emailService.UpdateEmailAsync(id, email);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmails([FromBody] SendEmailRequestDto request)
    {
        if (request == null || request.CompanyIds.Count == 0)
        {
            return BadRequest("Please provide a valid request with at least one Company ID.");
        }

        try
        {
            await _emailService.SendEmailsAsync(request);
            return Ok("Emails sent successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while sending emails: {ex.Message}");
        }
    }

    [HttpGet("country/{countryId}")]
    public async Task<IActionResult> GetByCountryId(int countryId, [FromQuery] int? positionId = null)
    {
        try
        {
            var emails = await _emailService.GetEmailsByCountryIdAsync(countryId, positionId);
            var emailDtos = emails.Select(e => new EmailDto
            {
                Id = e.Id,
                Subject = e.Subject,
                Content = e.Content,
                Attachment = e.Attachment,
                SentBy = e.SentBy,
                CountryId = e.CountryId,
                PositionId = e.PositionId
            });
            return Ok(emailDtos);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
