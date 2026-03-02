namespace DecisionMaker.Dtos.DecisionItem;


public class PostDecisionItemResponseDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int DecisionId { get; set; }
}