using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TextTemplating;

namespace DecisionMaker.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(50)]
        public required string Name { set; get; }
        [Required]
        [EmailAddress]
        public required string Email { set; get; }
        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
        public required string Password { set; get; }
    }
}