using Microsoft.AspNetCore.Mvc;
using Peluqueria.Application.Dtos.Account;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var newUserDto = await _accountService.LoginAsync(loginDto);
            return Ok(newUserDto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var newUserDto = await _accountService.RegisterAsync(registerDto);

            return Ok(newUserDto);
        }
    }
}