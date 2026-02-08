namespace JobOutreach.Application.DTO;

public class SendEmailRequestDto
{
    public List<int> CompanyIds { get; set; } = [];
    public int EmailTemplateId { get; set; }
}
