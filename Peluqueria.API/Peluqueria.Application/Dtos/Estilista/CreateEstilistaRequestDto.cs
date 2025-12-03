using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    /// <summary>
    /// Objeto de transferencia de datos para la creación de un nuevo estilista.
    /// </summary>
    /// <remarks>
    /// Este DTO se envía como `multipart/form-data` para permitir la subida de una imagen de perfil.
    /// Se crea automáticamente una cuenta de usuario en el sistema de identidad.
    /// </remarks>
    public class CreateEstilistaRequestDto
    {
        /// <summary>
        /// Nombre de usuario para el inicio de sesión. Debe ser único.
        /// </summary>
        /// <example>laura.estilista</example>
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 20 caracteres.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico corporativo o personal del estilista.
        /// </summary>
        /// <example>laura@peluqueria.com</example>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Ingrese un correo válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña inicial para la cuenta.
        /// </summary>
        /// <example>Peluqueria2025*</example>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre y apellido completos del profesional.
        /// </summary>
        /// <example>Laura Valencia</example>
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre debe tener entre 5 y 100 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Número de celular de contacto (Formato Colombia).
        /// </summary>
        /// <example>3001234567</example>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^3\d{9}$", ErrorMessage = "El teléfono debe ser un celular válido de Colombia (10 dígitos, iniciando con 3).")]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Lista de IDs de los servicios que este estilista puede realizar.
        /// </summary>
        /// <remarks>
        /// Debe seleccionarse al menos un servicio existente en la base de datos.
        /// </remarks>
        /// <example>[1, 2, 5]</example>
        [Required(ErrorMessage = "Debe seleccionar al menos un servicio.")]
        public List<int> ServiciosIds { get; set; } = new List<int>();

        /// <summary>
        /// (Opcional) Imagen de perfil del estilista. Formatos: .jpg, .png. Máx 5MB.
        /// </summary>
        public IFormFile? Imagen { get; set; }
    }
}