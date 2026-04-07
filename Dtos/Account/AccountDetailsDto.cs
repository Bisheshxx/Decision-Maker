using Microsoft.VisualStudio.TextTemplating;

namespace DecisionMaker.Dtos.Account
{
    public class AccountDetailsDto
    {
        public required string Id { set; get; }
        public required string Name { set; get; }
        public required string Email { set; get; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsOAuth { get; set; }
    }
}