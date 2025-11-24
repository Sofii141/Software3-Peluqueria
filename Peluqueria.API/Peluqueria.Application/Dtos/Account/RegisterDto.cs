using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? NombreCompleto { get; set; } // Nuevo
        [Required]
        [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Formato de teléfono no válido.")] // Ejemplo de validación de formato
        public string? Telefono { get; set; } // Nuevo
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}