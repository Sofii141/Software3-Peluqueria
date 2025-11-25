using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class BloqueoRangoDto
    {
        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime FechaFin { get; set; }

        [StringLength(200, ErrorMessage = "La razón no puede exceder los 200 caracteres.")]
        public string Razon { get; set; } = string.Empty;
    }
}