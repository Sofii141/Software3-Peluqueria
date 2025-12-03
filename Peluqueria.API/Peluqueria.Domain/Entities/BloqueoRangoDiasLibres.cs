namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Representa un bloqueo de calendario por fechas específicas (Vacaciones, Licencias, Incapacidades).
    /// </summary>
    /// <remarks>
    /// A diferencia del HorarioBase, esto aplica a fechas calendario concretas (ej. Del 1 al 15 de Enero).
    /// Tiene prioridad sobre el horario base (si es laborable pero hay bloqueo, no se trabaja).
    /// </remarks>
    public class BloqueoRangoDiasLibres
    {
        public int Id { get; set; }

        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;

        /// <summary>
        /// Fecha calendario donde inicia la ausencia.
        /// </summary>
        public DateTime FechaInicioBloqueo { get; set; }

        /// <summary>
        /// Fecha calendario donde termina la ausencia.
        /// </summary>
        public DateTime FechaFinBloqueo { get; set; }

        public string Razon { get; set; } = "Día Libre/Vacaciones";
    }
}