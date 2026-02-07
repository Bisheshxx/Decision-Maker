using System.ComponentModel.DataAnnotations;

namespace DecisionMaker.Account.LoginDto
{
    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}