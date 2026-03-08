using System.ComponentModel.DataAnnotations;

namespace DecisionMaker.Dtos.Account;


public class UpdateUserDto
{
    public required string? Name { set; get; }
    public string? ProfilePictureUrl { get; set; }
}

