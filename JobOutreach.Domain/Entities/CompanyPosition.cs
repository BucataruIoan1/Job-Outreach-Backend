namespace JobOutreach.Domain.Entities;

public class CompanyPosition
{
    public int CompanyId { get; set; }
    public int PositionId { get; set; }

    public Company Company { get; set; } = null!;
    public Position Position { get; set; } = null!;
}

