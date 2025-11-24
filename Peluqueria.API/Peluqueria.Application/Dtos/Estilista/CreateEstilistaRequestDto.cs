using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class CreateEstilistaRequestDto
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public string NombreCompleto { get; set; } = string.Empty;
        [Required] public string Telefono { get; set; } = string.Empty;
        [Required] public List<int> ServiciosIds { get; set; } = new List<int>(); // RNI-E003
        public IFormFile? Imagen { get; set; }
    }
}
