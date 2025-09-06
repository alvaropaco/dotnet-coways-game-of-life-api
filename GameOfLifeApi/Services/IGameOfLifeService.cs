using GameOfLifeApi.DTOs;

namespace GameOfLifeApi.Services;

public interface IGameOfLifeService
{
    bool[][] Next(bool[][] grid);
    bool[][] Step(bool[][] grid, int steps);
    FinalStateResponse ComputeConclusion(Guid id, bool[][] grid, int maxAttempts);
}
