using System;

namespace JobOutreach.Application.DTO;

public class EmailSentDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string SentToEmail { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
