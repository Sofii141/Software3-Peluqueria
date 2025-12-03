using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peluqueria.Application.Dtos.Account;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.API.Controllers
{
    /// <summary>
    /// Controlador encargado de la gestión de cuentas de usuario, autenticación (Login) y registro.
    /// </summary>
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AccountController"/>.
        /// </summary>
        /// <param name="accountService">Servicio de aplicación para la gestión de lógica de cuentas e identidad.</param>
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Inicia sesión en el sistema validando las credenciales del usuario.
        /// </summary>
        /// <param name="loginDto">Objeto de transferencia de datos con el nombre de usuario y contraseña.</param>
        /// <returns>Un objeto <see cref="NewUserDto"/> que contiene la información del usuario y el token JWT de acceso.</returns>
        /// <response code="200">Devuelve el usuario autenticado con su token.</response>
        /// <response code="400">Si las credenciales son inválidas o faltan datos.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(NewUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var newUserDto = await _accountService.LoginAsync(loginDto);
            return Ok(newUserDto);
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema, asignando el rol predeterminado de Cliente.
        /// </summary>
        /// <param name="registerDto">Objeto con los datos personales y credenciales para el nuevo usuario.</param>
        /// <returns>La confirmación de la creación del usuario.</returns>
        /// <response code="200">Si el usuario se registró exitosamente.</response>
        /// <response code="400">Si los datos son inválidos, el usuario ya existe o hubo un error en la validación de contraseñas.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var newUserDto = await _accountService.RegisterAsync(registerDto);

            return Ok(newUserDto);
        }

        /// <summary>
        /// Obtiene el listado de todos los usuarios registrados que poseen el rol de Cliente.
        /// </summary>
        /// <remarks>
        /// Este endpoint requiere autenticación y autorización con el rol de 'Admin'.
        /// </remarks>
        /// <returns>Una colección de usuarios con perfil de cliente.</returns>
        /// <response code="200">Devuelve la lista de clientes.</response>
        /// <response code="401">Si el usuario no está autenticado.</response>
        /// <response code="403">Si el usuario está autenticado pero no tiene permisos de Administrador.</response>
        [HttpGet("clientes")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _accountService.GetAllClientesAsync();
            return Ok(clientes);
        }
    }
}