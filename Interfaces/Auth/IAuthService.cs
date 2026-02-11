using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Response;

namespace DecisionMaker.Interfaces.Auth;

public interface IAuthService
{
    Task<ApiResponse<NewUserDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<object>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<NewUserDto>> RefreshAsync(RefreshDto refreshDto);
    Task<ApiResponse<object>> ConfirmEmailAsync(string userId, string token);
}