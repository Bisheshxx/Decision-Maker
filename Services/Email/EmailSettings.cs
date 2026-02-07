namespace DecisionMaker.Service.Email;

public class EmailSettings
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}
