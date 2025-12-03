namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Evento que notifica un bloqueo de calendario por fechas específicas (Vacaciones, Incapacidades).
    /// </summary>
    /// <remarks>
    /// Exchange: `agenda_exchange`. Routing Key: `bloqueo_dias.*`
    /// </remarks>
    public class BloqueoRangoDiasLibresEventDto
    {
        /// <summary>
        /// ID del estilista afectado.
        /// </summary>
        public int EstilistaId { get; set; }

        /// <summary>
        /// Fecha donde inicia la inactividad.
        /// </summary>
        public DateTime FechaInicioBloqueo { get; set; }

        /// <summary>
        /// Fecha donde finaliza la inactividad.
        /// </summary>
        public DateTime FechaFinBloqueo { get; set; }

        /// <summary>
        /// Acción realizada: "CREADO", "ACTUALIZADO", "ELIMINADO".
        /// </summary>
        public string Accion { get; set; } = string.Empty;
    }
}