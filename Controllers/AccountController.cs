using System.Security.Claims;
using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Error;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Helpers;
using DecisionMaker.Interfaces;
using DecisionMaker.Interfaces.Auth;
using DecisionMaker.Models;
using DecisionMaker.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


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
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse<LoginResponseDto>();
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result.Success)
            {
                CookieHelper.SetAuthCookies(Response, result.Data!.Token!, result.Data.RefreshToken!);
                return Ok(ApiResponse<UserDto>.Ok(new UserDto
                {
                    Id = result.Data.User.Id,
                    Name = result.Data.User.Name,
                    Email = result.Data.User.Email,
                    ProfilePictureUrl = result.Data.User.ProfilePictureUrl,
                }, "Login Successful!"));
            }
            return result.ToIActionResult(this);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse<LoginResponseDto>();
            }
            var result = await _authService.RegisterAsync(registerDto);
            return result.ToIActionResult(this);
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh(RefreshDto refreshDto)
        {
            var result = await _authService.RefreshAsync(refreshDto);
            return result.ToIActionResult(this);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            return result.ToIActionResult(this);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refresh_token = Request.Cookies["refresh_token"];
            var result = await _authService.LogoutAsync(refresh_token!);
            if (result.Success)
            {
                CookieHelper.RemoveAuthCookies(Response);
            }
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
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await _authService.HandleGoogleLoginAsync(Response);
            if (!result.Success)
                return BadRequest(result.Message);
            CookieHelper.SetAuthCookies(Response, result.Data!.Token!, result.Data.RefreshToken!);
            return Redirect(result.Data.RedirectUrl!);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(string id)
        {
            var userId = id ?? User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _authService.GetProfileAsync(userId);
            return result.ToIActionResult(this);
        }
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _authService.GetProfileAsync(userId);
            return result.ToIActionResult(this);
        }
        [HttpPut]
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