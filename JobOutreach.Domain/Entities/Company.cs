namespace JobOutreach.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string CompanyUrl { get; set; } = string.Empty;
    public bool IsContacted { get; set; }
    public int? CountryId { get; set; }
    public Country? Country { get; set; }
    public ICollection<Position> Positions { get; set; } = [];
}
