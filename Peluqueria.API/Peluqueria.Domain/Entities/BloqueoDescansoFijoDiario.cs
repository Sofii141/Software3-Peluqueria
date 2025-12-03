namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Representa un bloqueo de tiempo recurrente dentro de la jornada laboral (ej. Almuerzo).
    /// </summary>
    /// <remarks>
    /// El sistema impedirá agendar citas que se solapen con este rango horario en el día especificado.
    /// </remarks>
    public class BloqueoDescansoFijoDiario
    {
        public int Id { get; set; }

        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;

        /// <summary>
        /// Día de la semana en que aplica el descanso.
        /// </summary>
        public DayOfWeek DiaSemana { get; set; }

        public TimeSpan HoraInicioDescanso { get; set; }
        public TimeSpan HoraFinDescanso { get; set; }

        /// <summary>
        /// Descripción administrativa (ej. "Almuerzo", "Pausa activa").
        /// </summary>
        public string Razon { get; set; } = "Pausa/Almuerzo";
    }
}