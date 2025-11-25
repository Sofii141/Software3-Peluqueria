using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class CreateEstilistaRequestDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 20 caracteres.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Ingrese un correo válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre debe tener entre 5 y 100 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^3\d{9}$", ErrorMessage = "El teléfono debe ser un celular válido de Colombia (10 dígitos, iniciando con 3).")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar al menos un servicio.")]
        public List<int> ServiciosIds { get; set; } = new List<int>();

        public IFormFile? Imagen { get; set; }
    }
}