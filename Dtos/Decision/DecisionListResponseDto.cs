using DecisionMaker.Dtos.DecisionItem;

namespace DecisionMaker.Dtos.Decision;

public class DecisionListResponseDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DecisionItemResponseDto>? DecisionItems { get; set; }
}