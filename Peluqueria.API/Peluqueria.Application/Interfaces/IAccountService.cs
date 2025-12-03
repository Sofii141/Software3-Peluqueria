using Peluqueria.Application.Dtos.Account;
using Microsoft.AspNetCore.Identity;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Contrato para la gestión de cuentas de usuario, autenticación y registro.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Valida las credenciales de un usuario y genera un token JWT si son correctas.
        /// </summary>
        /// <param name="loginDto">Credenciales (Usuario y Password).</param>
        /// <returns>Objeto con el Token y datos del usuario, o null si falla.</returns>
        Task<NewUserDto?> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Registra un nuevo usuario en el sistema Identity y le asigna el rol de Cliente.
        /// </summary>
        /// <param name="registerDto">Datos del nuevo usuario.</param>
        /// <returns>Resultado de la operación de Identity.</returns>
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Genera el DTO de respuesta para un usuario ya autenticado (útil para refrescar sesión).
        /// </summary>
        Task<NewUserDto?> GetNewUserDto(string username, string password);

        /// <summary>
        /// Obtiene la lista de todos los usuarios con rol 'Cliente'.
        /// </summary>
        Task<IEnumerable<ClienteResponseDto>> GetAllClientesAsync();
    }
}