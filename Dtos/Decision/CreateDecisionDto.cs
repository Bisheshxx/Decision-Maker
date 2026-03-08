using System.ComponentModel.DataAnnotations;

namespace DecisionMaker.Dtos.Decision;

public class CreateDecisionDto
{
    [Required]
    public required string Title { get; set; }
    [MaxLength(250)]
    public string? Description { get; set; }
}