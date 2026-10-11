namespace JobOutreach.Domain.Entities;

public class CompanyPageSnapshot
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastChangedAt { get; set; }
    public int? LastStatusCode { get; set; }
    public string? LastError { get; set; }

    public Company? Company { get; set; }
}
