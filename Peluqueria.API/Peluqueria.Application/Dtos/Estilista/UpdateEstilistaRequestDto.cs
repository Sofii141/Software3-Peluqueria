using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class UpdateEstilistaRequestDto
    {
        [Required] public string NombreCompleto { get; set; } = string.Empty;
        [Required] public string Telefono { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        [Required] public List<int> ServiciosIds { get; set; } = new List<int>(); // RNI-E003
        public IFormFile? Imagen { get; set; }
    }
}
