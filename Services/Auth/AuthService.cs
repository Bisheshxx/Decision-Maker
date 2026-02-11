using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Interfaces;
using DecisionMaker.Interfaces.Auth;
using DecisionMaker.Models;
using DecisionMaker.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DecisionMaker.Services.Auth;

public class AuthServices : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<AppUser> _signinManager;
    private readonly IEmailSender _emailSender;
    private readonly AppSettings _appSettings;

    public AuthServices(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, IEmailSender emailSender, IOptions<AppSettings> appSettings)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signinManager = signInManager;
        _emailSender = emailSender;
        _appSettings = appSettings.Value;
    }

    public async Task<ApiResponse<NewUserDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);

        if (user == null)
        {
            return ApiResponse<NewUserDto>.Fail("User does not exist", ErrorType.Unauthorized);
        }

        var result = await _signinManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return ApiResponse<NewUserDto>.Fail("Incorrect Email or Password", ErrorType.Unauthorized);
        }
        var RefreshTokenGenerated = new RefreshToken
        {
            Token = _tokenService.GenerateRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };
        user.RefreshTokens.Add(RefreshTokenGenerated);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto
        {
            Id = user.Id!,
            Name = user.Name!,
            Email = user.Email!
        };
        var newUserDto = new NewUserDto
        {
            User = userDto,
            RefreshToken = RefreshTokenGenerated.Token,
            Token = _tokenService.CreateToken(user)
        };
        return ApiResponse<NewUserDto>.Ok(newUserDto, "Login Succesfull!");
    }

    public async Task<ApiResponse<object>> RegisterAsync(RegisterDto registerDto)
    {
        var ExistingUser = await _userManager.FindByEmailAsync(registerDto.Email!);

        if (ExistingUser != null)
        {
            return ApiResponse<object>.Fail("Email already exists", ErrorType.Conflict);
        }

        var user = new AppUser
        {
            Email = registerDto.Email,
            Name = registerDto.Name,
            UserName = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return ApiResponse<object>.Fail(result.Errors.Select(e => e.Description), ErrorType.ServerError);
        }
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = $"{_appSettings.FrontendUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendEmailAsync(user.Email!, "Confirm Email", $"Click <a href='{link}'>here</a> to confirm email");
        return ApiResponse<object>.Ok("Confirmation Email has been sent!");
    }

    public async Task<ApiResponse<NewUserDto>> RefreshAsync(RefreshDto refreshDto)
    {
        var user = await _userManager.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshDto.RefreshToken));
        if (user == null)
        {
            return ApiResponse<NewUserDto>.Fail("Invalid Refresh Token", ErrorType.Unauthorized);
        }
        var storeRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshDto.RefreshToken);

        if (storeRefreshToken == null)
        {
            return ApiResponse<NewUserDto>.Fail("Invalid Refresh Token", ErrorType.Unauthorized);
        }
        if (storeRefreshToken.IsExpired)
        {
            return ApiResponse<NewUserDto>.Fail("Refresh Token is Expired", ErrorType.Unauthorized);

        }
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
        var response = new NewUserDto
        {
            User = new UserDto
            {
                Id = user.Id!,
                Name = user.Name!,
                Email = user.Email!,
            },
            Token = newAccessToken,
            RefreshToken = newRefreshToken.Token
        };
        return ApiResponse<NewUserDto>.Ok(response, "Success");

        // return ApiResponse<NewUserDto>.Ok()
    }

    public async Task<ApiResponse<object>> ConfirmEmailAsync(string userId, string token)
    {

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<object>.Fail("User does not exist", ErrorType.NotFound);
        }
        if (user.EmailConfirmed)
        {
            return ApiResponse<object>.Fail("Email has already been confirmed", ErrorType.Conflict);
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return ApiResponse<object>.Ok("Email has been successfully confirmed");
        }
        else
        {
            return ApiResponse<object>.Fail("Email confirmation failed. The link may have expired or is invalid", ErrorType.Validation);
        }
    }

}