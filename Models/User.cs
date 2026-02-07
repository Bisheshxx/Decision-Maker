using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DecisionMaker.Models
{
    [Table("Users")]
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
        // Navigation property: One User can have many Decisions
        public List<Decision> Decisions { get; set; } = new List<Decision>();
        public List<RefreshToken> RefreshTokens { get; set; } = new();

    }


}