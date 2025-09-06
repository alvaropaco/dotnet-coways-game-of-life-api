namespace GameOfLifeApi.DTOs;

public class FinalStateResponse
{
    public Guid Id { get; set; }
    public bool[][] FinalGrid { get; set; } = Array.Empty<bool[]>();
    public int StepsTaken { get; set; }
    public bool IsLoop { get; set; }
    public int Period { get; set; }
    public string Conclusion { get; set; } = string.Empty;
}
