using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Account
{
    /// <summary>
    /// Objeto de transferencia de datos para el registro de nuevos usuarios en el sistema.
    /// Incluye validaciones de formato para garantizar la integridad de los datos.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Nombre de usuario deseado. Debe tener entre 3 y 20 caracteres.
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 20 caracteres.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario. Mínimo 5 caracteres.
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Ingrese su nombre y apellido completos (Mínimo 5 caracteres).")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Número de teléfono móvil. Debe cumplir con el formato de celular colombiano (10 dígitos, inicia con 3).
        /// </summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^3\d{9}$", ErrorMessage = "El teléfono debe ser un celular válido de Colombia (10 dígitos, iniciando con 3).")]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Dirección de correo electrónico válida.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Ingrese un correo válido (ejemplo: usuario@dominio.com).")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña para la nueva cuenta.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}