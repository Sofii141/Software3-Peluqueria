using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Account
{
    /// <summary>
    /// Objeto de transferencia de datos que contiene las credenciales necesarias para iniciar sesión.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Nombre de usuario para la autenticación.
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}