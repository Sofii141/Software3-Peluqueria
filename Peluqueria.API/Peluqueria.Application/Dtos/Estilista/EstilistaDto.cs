using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class EstilistaDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
        public string Telefono { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
        public List<int> ServiciosIds { get; set; } = new List<int>();
    }
}
