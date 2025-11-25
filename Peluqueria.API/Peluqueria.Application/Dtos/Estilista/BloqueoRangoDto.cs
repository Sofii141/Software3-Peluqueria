using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class BloqueoRangoDto // Para BloqueoRangoDiasLibres
    {
        [Required] public DateTime FechaInicio { get; set; }
        [Required] public DateTime FechaFin { get; set; }
        public string Razon { get; set; } = string.Empty;
    }
}
