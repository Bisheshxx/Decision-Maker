namespace DecisionMaker.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string? Token { get; set; }
        public DateTime Expires { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;

        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }

}