namespace JobOutreach.Domain.Entities;

public class Email
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Attachment { get; set; } = string.Empty;
    public string SentBy { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public int? PositionId { get; set; }

    public Country? Country { get; set; }
    public Position? Position { get; set; }
}
