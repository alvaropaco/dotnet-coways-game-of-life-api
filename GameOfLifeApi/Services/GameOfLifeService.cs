using GameOfLifeApi.DTOs;
using GameOfLifeApi.Utils;

namespace GameOfLifeApi.Services;

public class GameOfLifeService : IGameOfLifeService
{
    public bool[][] Next(bool[][] grid)
    {
        var height = grid.Length;
        var width = grid[0].Length;
        var next = new bool[height][];
        for (int r = 0; r < height; r++)
        {
            var row = new bool[width];
            next[r] = row;
            for (int c = 0; c < width; c++)
            {
                int neighbors = 0;
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int nr = r + dr, nc = c + dc;
                        if (nr >= 0 && nr < height && nc >= 0 && nc < width && grid[nr][nc])
                            neighbors++;
                    }
                }
                if (grid[r][c])
                {
                    row[c] = neighbors == 2 || neighbors == 3;
                }
                else
                {
                    row[c] = neighbors == 3;
                }
            }
        }
        return next;
    }

    public bool[][] Step(bool[][] grid, int steps)
    {
        if (steps < 0) throw new ArgumentOutOfRangeException(nameof(steps));
        var current = GridUtils.Clone(grid);
        for (int i = 0; i < steps; i++)
            current = Next(current);
        return current;
    }

    public FinalStateResponse ComputeConclusion(Guid id, bool[][] grid, int maxAttempts)
    {
        if (maxAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        var seen = new Dictionary<string, int>();
        var current = GridUtils.Clone(grid);
        var serialized = GridUtils.Serialize(current);
        seen[serialized] = 0;

        for (int step = 1; step <= maxAttempts; step++)
        {
            current = Next(current);
            serialized = GridUtils.Serialize(current);

            if (seen.TryGetValue(serialized, out var firstSeenAt))
            {
                var period = step - firstSeenAt;
                return new FinalStateResponse
                {
                    Id = id,
                    FinalGrid = current,
                    StepsTaken = step,
                    IsLoop = true,
                    Period = period,
                    Conclusion = period == 1 ? "Stable (fixed point)" : $"Loop detected (period {period})"
                };
            }
            seen[serialized] = step;
        }
        throw new InvalidOperationException($"Conclusion not reached within {maxAttempts} attempts.");
    }
}
