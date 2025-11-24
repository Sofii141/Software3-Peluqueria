using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class BloqueoRangoDto // Para BloqueoRangoDiasLibres
    {
        [Required] public DateTime FechaInicio { get; set; }
        [Required] public DateTime FechaFin { get; set; }
        public string Razon { get; set; } = string.Empty;
    }
}
