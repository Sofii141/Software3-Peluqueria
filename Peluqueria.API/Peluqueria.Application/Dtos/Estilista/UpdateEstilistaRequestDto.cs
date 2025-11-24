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
        // La contraseña se maneja aparte, y el email/username no se editan (RNI-S005)
        [Required] public List<int> ServiciosIds { get; set; } = new List<int>(); // RNI-E003
    }
}
