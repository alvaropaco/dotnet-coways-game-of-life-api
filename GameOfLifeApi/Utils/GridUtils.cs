namespace GameOfLifeApi.Utils;

public static class GridUtils
{
    public static (bool ok, string? error) ValidateRectangular(bool[][]? grid)
    {
        if (grid is null) return (false, "Grid must not be null.");
        if (grid.Length == 0) return (false, "Grid must have at least one row.");
        var width = grid[0]?.Length ?? -1;
        if (width <= 0) return (false, "Grid must have at least one column.");
        for (int r = 0; r < grid.Length; r++)
        {
            if (grid[r] is null) return (false, $"Row {r} is null.");
            if (grid[r].Length != width)
                return (false, $"All rows must have the same length. Row 0 has {width}, row {r} has {grid[r].Length}.");
        }
        return (true, null);
    }

    public static int AliveCount(bool[][] grid)
    {
        var count = 0;
        for (int r = 0; r < grid.Length; r++)
            for (int c = 0; c < grid[r].Length; c++)
                if (grid[r][c]) count++;
        return count;
    }

    public static string Serialize(bool[][] grid)
    {
        var rows = new string[grid.Length];
        for (int r = 0; r < grid.Length; r++)
        {
            var span = grid[r];
            var chars = new char[span.Length];
            for (int c = 0; c < span.Length; c++)
                chars[c] = span[c] ? '1' : '0';
            rows[r] = new string(chars);
        }
        return string.Join(';', rows);
    }

    public static bool[][] Clone(bool[][] grid)
    {
        var clone = new bool[grid.Length][];
        for (int r = 0; r < grid.Length; r++)
        {
            clone[r] = new bool[grid[r].Length];
            Array.Copy(grid[r], clone[r], grid[r].Length);
        }
        return clone;
    }
}
