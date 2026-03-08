using System.ComponentModel.DataAnnotations;

namespace DecisionMaker.Dtos.DecisionItem;


public class UpdateDecisionItemDto
{
    [Required]
    [MaxLength(50)]
    public required string Title { get; set; }
}