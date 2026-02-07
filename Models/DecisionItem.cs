using System.ComponentModel.DataAnnotations.Schema;

namespace DecisionMaker.Models
{
    public class DecisionItem
    {
        public int Id { get; set; }                  // PK
        public required string Title { get; set; }

        // Foreign key: Decision
        public int DecisionId { get; set; }
        public Decision? Decision { get; set; }      // Navigation property
    }
}
