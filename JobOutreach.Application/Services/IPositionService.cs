using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public interface IPositionService
{
    Task<IEnumerable<Position>> GetAllPositionsAsync();
    Task<Position?> GetPositionByIdAsync(int id);
    Task<IEnumerable<Position>> GetPositionsByCountryIdAsync(int countryId);
    Task<Position> CreatePositionAsync(Position position);
    Task UpdatePositionAsync(int id, Position position);
    Task DeletePositionAsync(int id);
}

