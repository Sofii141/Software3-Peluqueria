using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Define la plantilla de horario laboral recurrente de un estilista.
    /// </summary>
    /// <remarks>
    /// No representa fechas específicas (ej. 12 de Octubre), sino días de la semana (ej. "Todos los Lunes").
    /// </remarks>
    public class HorarioSemanalBase
    {
        public int Id { get; set; }

        /// <summary>
        /// El estilista dueño de este horario.
        /// </summary>
        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;

        /// <summary>
        /// El día de la semana al que aplica esta regla.
        /// </summary>
        [Required]
        public DayOfWeek DiaSemana { get; set; }

        /// <summary>
        /// Hora de apertura/entrada.
        /// </summary>
        public TimeSpan HoraInicioJornada { get; set; }

        /// <summary>
        /// Hora de cierre/salida.
        /// </summary>
        public TimeSpan HoraFinJornada { get; set; }

        /// <summary>
        /// Indica si el estilista trabaja este día. Si es false, se considera día libre recurrente.
        /// </summary>
        public bool EsLaborable { get; set; } = true;
    }
}