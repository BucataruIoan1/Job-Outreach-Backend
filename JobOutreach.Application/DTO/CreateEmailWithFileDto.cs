using Microsoft.AspNetCore.Http;

namespace JobOutreach.Application.DTO;

public class CreateEmailWithFileDto
{
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public int? PositionId { get; set; }
    public IFormFile? PdfFile { get; set; }
}
