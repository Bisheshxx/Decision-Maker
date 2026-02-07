using static DecisionMaker.Models.AppUser;

namespace DecisionMaker.Models
{
    public class Decision
    {
        public int Id { get; set; }                  // PK
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? UserId { get; set; }
        public AppUser? AppUser { get; set; }

        public List<DecisionItem> DecisionItems { get; set; } = new List<DecisionItem>();

    }
}