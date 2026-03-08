namespace DecisionMaker.Dtos.Account;

public class OAuthLoginDto
{
    public UserDto? User { get; set; }
    public required string RedirectUrl { get; set; }

}