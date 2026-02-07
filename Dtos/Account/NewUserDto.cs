namespace DecisionMaker.Dtos.Account
{
    public class NewUserDto
    {
        public required UserDto User { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }


    }
}