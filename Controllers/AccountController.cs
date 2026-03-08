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
                return Ok(ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
                {
                    User = result.Data.User
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

        [HttpPost("confirm-email/{userId}")]
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
        public IActionResult GoogleLogin(string? returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/auth/google-response"
            };

            properties.Items["returnUrl"] = returnUrl ?? "/";
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync();
            var returnUrl = authenticateResult.Properties?.Items["returnUrl"] ?? "/";

            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            var result = await _authService.HandleGoogleLoginAsync(Response);

            if (!result.Success)
                return BadRequest(result.Message);

            CookieHelper.SetAuthCookies(Response, result.Data!.Token!, result.Data.RefreshToken!);

            return Ok(ApiResponse<OAuthLoginDto>.Ok(new OAuthLoginDto
            {
                User = result.Data.User,
                RedirectUrl = returnUrl
            }, "Login Successful!"));
        }
    }
}