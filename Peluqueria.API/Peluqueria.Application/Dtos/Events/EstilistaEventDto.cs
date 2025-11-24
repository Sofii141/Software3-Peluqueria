using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Events
{
    public class EstilistaEventDto
    {
        public int Id { get; set; }
        public string IdentityId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
        public List<EstilistaServicioMinimalEventDto> ServiciosAsociados { get; set; } = new List<EstilistaServicioMinimalEventDto>();

        public string Accion { get; set; } = string.Empty; // CREADO, ACTUALIZADO, INACTIVADO
    }
}
