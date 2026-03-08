using System.ComponentModel.DataAnnotations;

namespace DecisionMaker.Dtos.DecisionItem;


public class CreateDecisionItemDto
{
    // public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public required string Title { get; set; }
    // public int DecisionId { get; set; }
}