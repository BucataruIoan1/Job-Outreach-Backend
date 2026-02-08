using System;

namespace JobOutreach.Domain.Entities;

public class EmailSent
{
    public int Id { get; set; }
    public int EmailId { get; set; }
    public int CompanyId { get; set; }
    public int PositionId { get; set; }
    public string SentToEmail { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Email? Email { get; set; }
    public Company? Company { get; set; }
    public Position? Position { get; set; }
}
