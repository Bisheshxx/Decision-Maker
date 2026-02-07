using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Error;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Interfaces;
using DecisionMaker.Models;
using DecisionMaker.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace DecisionMaker.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signinManager;
        private readonly IEmailSender _emailSender;
        private readonly AppSettings _appSettings;


        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, IEmailSender emailSender, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signinManager = signInManager;
            _emailSender = emailSender;
            _appSettings = appSettings.Value;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage), "ModelState is invalid"));
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null)
                return Unauthorized(ApiResponse<object>.Fail("The User does not exist"));

            var result = await _signinManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(ApiResponse<object>.Fail("Email Notfound or Password doesn't match!"));


            var RefreshToken = new RefreshToken
            {
                Token = _tokenService.GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id

            };
            user.RefreshTokens.Add(RefreshToken);
            await _userManager.UpdateAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id!,
                Name = user.Name!,
                Email = user.Email!,
            };
            return Ok(ApiResponse<UserDto>.Ok(userDto, "Login successful"));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage), "ModelState is invalid"));

            if (string.IsNullOrWhiteSpace(registerDto.Email))
                return BadRequest(ApiResponse<object>.Fail("Email required"));

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest(ApiResponse<object>.Fail("Email Already Exists!"));
            var appUser = new AppUser
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                UserName = Guid.NewGuid().ToString()
            };
            var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);
            if (createdUser.Succeeded)
            {
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

                var frontendUrl = _appSettings.FrontendUrl;
                var confirmationLink = $"{frontendUrl}/confirm-email?userId={appUser.Id}&token={Uri.EscapeDataString(emailToken)}";


                await _emailSender.SendEmailAsync(appUser.Email, "Confirm Your Email", $"<h3>Hello {appUser.Name}</h3><p>Please confirm your email by clicking this link:</p><a href='{confirmationLink}'>Confirm Email</a>");

                return Ok(ApiResponse<object>.Ok(null, "Email sent"));
            }
            else
            {
                var errors = createdUser.Errors.Select(e => e.Description);
                return BadRequest(ApiResponse<object>.Fail(errors, "Registration failed"));
            }
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh(RefreshDto refreshDto)
        {
            var user = await _userManager.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshDto.RefreshToken));
            if (user == null)
                return Unauthorized("Invalid refresh Token");

            var storeRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshDto.RefreshToken);
            if (storeRefreshToken == null)
                return Unauthorized("Invalid refresh Token");

            if (storeRefreshToken.IsExpired)
                return Unauthorized("Refresh Token is Expired");

            user.RefreshTokens.Remove(storeRefreshToken);
            var newRefreshToken = new RefreshToken
            {
                Token = _tokenService.GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var newAccessToken = _tokenService.CreateToken(user);

            return Ok(
                new NewUserDto
                {
                    User = new UserDto
                    {
                        Id = user.Id!,
                        Name = user.Name!,
                        Email = user.Email!,
                    },
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken.Token
                }
            );
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User does not exist");
            if (user.EmailConfirmed)
            {
                return Conflict("Email has already been confirmed");
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("Email has successfully been confirmed");
            }
            else
            {
                return BadRequest("Email confirmation failed. The link may have expired or is invalid.");
            }
        }
    }
}