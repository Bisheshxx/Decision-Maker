using DecisionMaker.Models;

namespace DecisionMaker.Dtos.Decision;

public class DecisionDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? UserId { get; set; }
}