namespace GameOfLifeApi.DTOs;

public class BoardStateResponse
{
    public Guid Id { get; set; }
    public int Generation { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int AliveCount { get; set; }
    public bool[][] Grid { get; set; } = Array.Empty<bool[]>();
}
