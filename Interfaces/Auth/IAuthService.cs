using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Response;
using Microsoft.AspNetCore.Authentication;

namespace DecisionMaker.Interfaces.Auth;

public interface IAuthService
{
    Task<ApiResponse<NewUserDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<object>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<NewUserDto>> RefreshAsync(string refreshDto);
    Task<ApiResponse<object>> ConfirmEmailAsync(string userId, string token);
    Task<ApiResponse<object>> LogoutAsync(string refresh_token);
    // AuthenticationProperties ConfigureExternalAuth(string provider, string redirectUrl);
    AuthenticationProperties ConfigureGoogleLogin(string redirectUrl);

    Task<ApiResponse<NewUserDto>> HandleGoogleLoginAsync(HttpResponse response);
    Task<ApiResponse<AccountDetailsDto>> GetProfileAsync(string id);
    Task<ApiResponse<UserDto>> UpdateProfile(string id, UpdateUserDto updateUserDto);

    Task<ApiResponse<object>> UpdatePasswordAsync(PasswordUpdateDto passwordUpdateDto, string id);


}