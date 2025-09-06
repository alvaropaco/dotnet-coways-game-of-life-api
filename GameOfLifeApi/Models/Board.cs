using LiteDB;

namespace GameOfLifeApi.Models;

public class Board
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Generation { get; set; } = 0;
    public bool[][] Grid { get; set; } = Array.Empty<bool[]>();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
