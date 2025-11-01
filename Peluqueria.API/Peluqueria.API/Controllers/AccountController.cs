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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Delega toda la lógica de validación, búsqueda de usuario y generación de token
            var newUserDto = await _accountService.LoginAsync(loginDto);

            // El servicio devuelve null si el login es inválido
            if (newUserDto == null)
                return Unauthorized("Username not found and/or password incorrect");

            return Ok(newUserDto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (registerDto.Password == null)
                {
                    return BadRequest("La contraseña es requerida.");
                }

                // Delega el registro, creación de usuario y asignación de rol
                var createdResult = await _accountService.RegisterAsync(registerDto);

                if (createdResult.Succeeded)
                {
                    // Si el registro y la asignación de rol fueron exitosos, obtenemos el token
                    var newUserDto = await _accountService.GetNewUserDto(registerDto.Username!, registerDto.Password);
                    return Ok(newUserDto);
                }
                else
                {
                    // Si falla por validación de Identity (ej. usuario ya existe), es Bad Request (400)
                    // Si falla la asignación de rol, la lógica se mueve al servicio, pero aquí devolvemos el error
                    return BadRequest(createdResult.Errors);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}