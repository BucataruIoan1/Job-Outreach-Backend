namespace JobOutreach.Application.DTO;

public class EmailDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Attachment { get; set; } = string.Empty;
    public string SentBy { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public int? PositionId { get; set; }
}
