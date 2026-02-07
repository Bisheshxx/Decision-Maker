using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Error;
using DecisionMaker.Interfaces;
using DecisionMaker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DecisionMaker.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signinManager;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signinManager = signInManager;
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
                    return BadRequest(ModelState);
                var existingUser = await _userManager.Users
                                .FirstOrDefaultAsync(u => string.Equals(u.Email, registerDto.Email, StringComparison.OrdinalIgnoreCase));


                if (existingUser != null)
                    return BadRequest(new { message = "Email is already registered" });
                var appUser = new AppUser
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    UserName = Guid.NewGuid().ToString()
                };

                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

                if (createdUser.Succeeded)
                {
                    var refreshToken = new RefreshToken
                    {
                        Token = _tokenService.GenerateRefreshToken(),
                        Expires = DateTime.UtcNow.AddDays(7),
                        UserId = appUser.Id
                    };

                    appUser.RefreshTokens.Add(refreshToken);
                    await _userManager.UpdateAsync(appUser);

                    return Ok(
                        new NewUserDto
                        {
                            User = new UserDto
                            {
                                Id = appUser.Id!,
                                Name = appUser.Name!,
                                Email = appUser.Email!,
                            },
                            Token = _tokenService.CreateToken(appUser),
                            RefreshToken = refreshToken.Token
                        }
                    );
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
                return StatusCode(500, error); // Safe to serialize
                // return StatusCode(500, e);
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
                Token = _tokenService.GenerateRefreshToken()
                ,
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
    }
}