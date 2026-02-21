namespace DecisionMaker.Dtos.Decision;

public class CreateDecisionDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
}