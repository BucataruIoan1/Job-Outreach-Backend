namespace JobOutreach.Application.DTO;

public class UpdateCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string CompanyUrl { get; set; } = string.Empty;
    public int? CountryId { get; set; }
    public List<int>? PositionIds { get; set; }
}
