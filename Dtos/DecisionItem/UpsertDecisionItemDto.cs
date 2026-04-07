using System.ComponentModel.DataAnnotations;

namespace DecisionMaker.Dtos.Decision;

public class UpsertDecisionItemDto
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public required string Title { get; set; }
}