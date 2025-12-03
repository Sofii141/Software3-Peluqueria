using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    /// <summary>
    /// Objeto para actualizar la información de un estilista existente.
    /// </summary>
    /// <remarks>
    /// Los campos como Password, Username o Email son opcionales. 
    /// Envíelos solo si desea modificarlos; de lo contrario, déjelos vacíos o nulos.
    /// </remarks>
    public class UpdateEstilistaRequestDto
    {
        /// <summary>
        /// Nuevo nombre completo.
        /// </summary>
        /// <example>Laura Valencia de Pérez</example>
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre debe tener entre 5 y 100 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Nuevo teléfono de contacto.
        /// </summary>
        /// <example>3109876543</example>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^3\d{9}$", ErrorMessage = "El teléfono debe ser un celular válido de Colombia.")]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Nueva contraseña. Si se deja vacío, la contraseña actual se mantiene.
        /// </summary>
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string? Password { get; set; }

        /// <summary>
        /// (Opcional) Nuevo nombre de usuario.
        /// </summary>
        [StringLength(20, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 20 caracteres.")]
        public string? Username { get; set; }

        /// <summary>
        /// (Opcional) Nuevo correo electrónico.
        /// </summary>
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Ingrese un correo válido.")]
        public string? Email { get; set; }

        /// <summary>
        /// Lista completa de servicios que realizará el estilista.
        /// </summary>
        /// <remarks>
        /// Esta lista SOBRESCRIBE la anterior. Si quiere mantener los servicios previos, debe enviarlos nuevamente.
        /// </remarks>
        /// <example>[1, 3]</example>
        [Required(ErrorMessage = "Debe mantener al menos un servicio asociado.")]
        public List<int> ServiciosIds { get; set; } = new List<int>();

        /// <summary>
        /// (Opcional) Nueva imagen de perfil para reemplazar la actual.
        /// </summary>
        public IFormFile? Imagen { get; set; }
    }
}