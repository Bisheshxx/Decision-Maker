using Microsoft.VisualStudio.TextTemplating;

namespace DecisionMaker.Dtos.Account
{
    public class UserDto
    {
        public required string Id { set; get; }
        public required string Name { set; get; }
        public required string Email { set; get; }
    }
}