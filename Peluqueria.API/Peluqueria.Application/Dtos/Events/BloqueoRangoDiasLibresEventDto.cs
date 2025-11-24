using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Events
{
    public class BloqueoRangoDiasLibresEventDto
    {
        public int EstilistaId { get; set; } 
        public DateTime FechaInicioBloqueo { get; set; } 
        public DateTime FechaFinBloqueo { get; set; }
        public string Accion { get; set; } = string.Empty; 
    }
}
