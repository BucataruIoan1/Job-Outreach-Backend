using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public class PositionService(IPositionRepository positionRepository) : IPositionService
{
    private readonly IPositionRepository _positionRepository = positionRepository;

    public async Task<IEnumerable<Position>> GetAllPositionsAsync()
        => await _positionRepository.GetAllAsync();

    public async Task<Position?> GetPositionByIdAsync(int id)
        => await _positionRepository.GetByIdAsync(id);

    public async Task<IEnumerable<Position>> GetPositionsByCountryIdAsync(int countryId)
        => await _positionRepository.GetByCountryIdAsync(countryId);

    public async Task<Position> CreatePositionAsync(Position position)
        => await _positionRepository.AddAsync(position);

    public async Task UpdatePositionAsync(int id, Position position)
    {
        var existingPosition = await _positionRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Position with Id {id} not found.");

        existingPosition.PositionName = position.PositionName;
        await _positionRepository.UpdateAsync(existingPosition);
    }

    public async Task DeletePositionAsync(int id)
    {
        _ = await _positionRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Position with Id {id} not found.");

        await _positionRepository.DeleteAsync(id);
    }
}

