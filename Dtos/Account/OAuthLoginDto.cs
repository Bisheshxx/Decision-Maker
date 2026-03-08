namespace DecisionMaker.Dtos.Account;

public class OAuthLoginDto
{
    public UserDto? User { get; set; }
    public string? RedirectUrl { get; set; }
}