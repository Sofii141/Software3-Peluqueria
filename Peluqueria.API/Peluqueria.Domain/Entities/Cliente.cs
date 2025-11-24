using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Domain.Entities
{
    public class Cliente 
    {
        public int Id { get; set; }
        [Required]
        public string IdentityId { get; set; } = string.Empty; // Enlace con AppUser (via Infraestructura)
        [Required]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required]
        public string Telefono { get; set; } = string.Empty;
    }
}
