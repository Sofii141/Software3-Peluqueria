using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class HorarioDiaDto
    {
        [Required(ErrorMessage = "El día de la semana es obligatorio.")]
        public DayOfWeek DiaSemana { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        public TimeSpan HoraFin { get; set; }

        public bool EsLaborable { get; set; } = true;
    }
}