namespace DecisionMaker.Dtos.DecisionItem;


public class UpdateDecisionItemDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int DecisionId { get; set; }
}