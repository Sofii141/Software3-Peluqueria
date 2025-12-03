namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Objeto auxiliar que define un rango horario en un día específico de la semana.
    /// </summary>
    public class DiaHorarioEventDto
    {
        /// <summary>
        /// Día de la semana (Sunday=0, Monday=1, etc).
        /// </summary>
        public DayOfWeek DiaSemana { get; set; }

        /// <summary>
        /// Hora de inicio del rango.
        /// </summary>
        public TimeSpan HoraInicio { get; set; }

        /// <summary>
        /// Hora de fin del rango.
        /// </summary>
        public TimeSpan HoraFin { get; set; }

        /// <summary>
        /// Indica si es tiempo de trabajo (true) o tiempo libre/descanso (false).
        /// </summary>
        public bool EsLaborable { get; set; }
    }
}