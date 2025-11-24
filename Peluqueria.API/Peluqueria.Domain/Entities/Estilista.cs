using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Domain.Entities
{
    public class Estilista 
    {
        public int Id { get; set; }
        public string IdentityId { get; set; } = string.Empty; // Enlace con AppUser (via Infraestructura)
        public string NombreCompleto { get; set; } = string.Empty; // Copia de datos para el dominio
        public string Telefono { get; set; } = string.Empty;
        public bool EstaActivo { get; set; } = true; // Baja Lógica (PEL-HU-11)

        // Relación M:N con Servicio (PEL-HU-09)
        public ICollection<EstilistaServicio> ServiciosAsociados { get; set; } = new List<EstilistaServicio>();

    }
}
