namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Evento que define la jornada laboral estándar semanal de un estilista.
    /// </summary>
    /// <remarks>
    /// Se envía cuando el administrador configura el horario base (ej: Lunes a Viernes de 9 a 6).
    /// Exchange: `agenda_exchange`.
    /// </remarks>
    public class HorarioBaseEstilistaEventDto
    {
        /// <summary>
        /// ID del estilista dueño del horario.
        /// </summary>
        public int EstilistaId { get; set; }

        /// <summary>
        /// Lista de configuraciones por día de la semana.
        /// </summary>
        public List<DiaHorarioEventDto> HorariosSemanales { get; set; } = new List<DiaHorarioEventDto>();
    }
}