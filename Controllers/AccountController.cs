using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Error;
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
                return BadRequest(ModelState);
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid Email!");
            var result = await _signinManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Email Notfound or Password doesn't match!");

            var RefreshToken = new RefreshToken
            {
                Token = _tokenService.GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id

            };
            user.RefreshTokens.Add(RefreshToken);
            await _userManager.UpdateAsync(user);

            return Ok(
                new NewUserDto
                {
                    User = new UserDto
                    {
                        Id = user.Id!,
                        Name = user.Name!,
                        Email = user.Email!,
                    },
                    Token = _tokenService.CreateToken(user),
                    RefreshToken = RefreshToken.Token
                }
            );
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Model State");
                if (string.IsNullOrWhiteSpace(registerDto.Email))
                    return BadRequest("Email required");
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                    return BadRequest("Email Already Exists!");
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
                    return Ok(new
                    {
                        message = "Email sent",
                    });
                }
                else
                {
                    return StatusCode(500, createdUser.Errors);
                }

            }
            catch (Exception e)
            {
                var error = new ErrorResponse
                {
                    Message = e.Message,
                    StackTrace = e.StackTrace
                };

                return StatusCode(500, error);
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
            try
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
            catch (Exception e)
            {
                var error = new ErrorResponse
                {
                    Message = e.Message,
                    StackTrace = e.StackTrace

                };
                return StatusCode(500, error);
            }
        }
    }
}