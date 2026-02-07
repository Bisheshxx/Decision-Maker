using DecisionMaker.Models;

namespace DecisionMaker.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
        string GenerateRefreshToken();
    }
}