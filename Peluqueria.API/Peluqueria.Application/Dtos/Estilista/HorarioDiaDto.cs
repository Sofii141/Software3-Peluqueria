using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Estilista
{
    /// <summary>
    /// Define la configuración de horario para un día específico de la semana.
    /// </summary>
    public class HorarioDiaDto
    {
        /// <summary>
        /// Día de la semana (0 = Domingo, 1 = Lunes, ..., 6 = Sábado).
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El día de la semana es obligatorio.")]
        public DayOfWeek DiaSemana { get; set; }

        /// <summary>
        /// Hora de inicio de la jornada o descanso (Formato HH:mm:ss).
        /// </summary>
        /// <example>09:00:00</example>
        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeSpan HoraInicio { get; set; }

        /// <summary>
        /// Hora de finalización de la jornada o descanso (Formato HH:mm:ss).
        /// </summary>
        /// <example>18:00:00</example>
        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        public TimeSpan HoraFin { get; set; }

        /// <summary>
        /// Indica si el estilista trabaja este día. Si es false, se considera día libre.
        /// </summary>
        /// <example>true</example>
        public bool EsLaborable { get; set; } = true;
    }
}