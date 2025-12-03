namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Evento que notifica la actualización de los descansos fijos (ej. hora de almuerzo).
    /// </summary>
    public class DescansoFijoActualizadoEventDto
    {
        /// <summary>
        /// ID del estilista.
        /// </summary>
        public int EstilistaId { get; set; }

        /// <summary>
        /// Lista de franjas horarias bloqueadas por descanso recurrente.
        /// </summary>
        public List<DiaHorarioEventDto> DescansosFijos { get; set; } = new List<DiaHorarioEventDto>();
    }
}