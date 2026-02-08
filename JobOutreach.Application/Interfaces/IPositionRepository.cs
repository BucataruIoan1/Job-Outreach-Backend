using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Interfaces;

public interface IPositionRepository
{
    Task<IEnumerable<Position>> GetAllAsync();
    Task<Position?> GetByIdAsync(int id);
    Task<IEnumerable<Position>> GetByCountryIdAsync(int countryId);
    Task<Position> AddAsync(Position position);
    Task UpdateAsync(Position position);
    Task DeleteAsync(int id);
}

