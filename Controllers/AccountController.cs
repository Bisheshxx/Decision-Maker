using System.Security.Claims;
using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Error;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Helpers;
using DecisionMaker.Interfaces;
using DecisionMaker.Interfaces.Auth;
using DecisionMaker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace DecisionMaker.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<NewUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse<LoginResponseDto>();
            }

            var result = await _authService.LoginAsync(loginDto);
            return result.ToIActionResult(this);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse<LoginResponseDto>();
            }
            var result = await _authService.RegisterAsync(registerDto);
            return result.ToIActionResult(this);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<NewUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto refreshDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse<RefreshDto>();
            }
            var result = await _authService.RefreshAsync(refreshDto.RefreshToken!);
            return result.ToIActionResult(this);
        }

        [HttpPost("confirm-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            return result.ToIActionResult(this);
        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] RefreshDto refreshDto)
        {
            var result = await _authService.LogoutAsync(refreshDto.RefreshToken!);
            return result.ToIActionResult(this);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl)
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse));
            var properties = _authService.ConfigureGoogleLogin(redirectUrl!);
            properties.Items["returnUrl"] = returnUrl ?? "/";

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet("google-response")]
        [ProducesResponseType(typeof(ApiResponse<NewUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await _authService.HandleGoogleLoginAsync(Response);
            if (!result.Success)
                return BadRequest(result.Message);

            var redirectUrl = result.Data!.RedirectUrl ?? "/";
            var redirectWithTokens = BuildRedirectUrlWithTokens(
                redirectUrl,
                result.Data.Token,
                result.Data.RefreshToken);

            return Redirect(redirectWithTokens);
        }

        private static string BuildRedirectUrlWithTokens(string baseUrl, string? token, string? refreshToken)
        {
            var encodedToken = Uri.EscapeDataString(token ?? string.Empty);
            var encodedRefreshToken = Uri.EscapeDataString(refreshToken ?? string.Empty);
            var fragment = $"accessToken={encodedToken}&refreshToken={encodedRefreshToken}";

            return baseUrl.Contains('#')
                ? $"{baseUrl}&{fragment}"
                : $"{baseUrl}#{fragment}";
        }
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile(string id)
        {
            var userId = id ?? User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _authService.GetProfileAsync(userId);
            return result.ToIActionResult(this);
        }
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _authService.GetProfileAsync(userId);
            return result.ToIActionResult(this);
        }
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse<LoginResponseDto>();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _authService.UpdateProfile(userId, updateUserDto);
            return result.ToIActionResult(this);
        }

        [HttpPut("password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePassword(PasswordUpdateDto passwordUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse<PasswordUpdateDto>();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _authService.UpdatePasswordAsync(passwordUpdateDto, userId);
            return result.ToIActionResult(this);
        }
    }
}