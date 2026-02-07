using Microsoft.VisualStudio.TextTemplating;

namespace DecisionMaker.Dtos.Account
{
    public class RegisterDto
    {
        public required string Name { set; get; }
        public required string Email { set; get; }
        public required string Password { set; get; }
    }
}