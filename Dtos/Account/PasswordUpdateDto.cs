using System.ComponentModel.DataAnnotations;

namespace DecisionMaker.Dtos.Account;

public class PasswordUpdateDto
{
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
    public required string OldPassword { set; get; }
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
    public required string NewPassword { set; get; }
}