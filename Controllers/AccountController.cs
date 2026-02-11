using DecisionMaker.Account.LoginDto;
using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Error;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Helpers;
using DecisionMaker.Interfaces;
using DecisionMaker.Interfaces.Auth;
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
        private readonly IAuthService _authService;


        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            return result.ToIActionResult(this);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
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
    }
}