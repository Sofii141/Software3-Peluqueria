using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Servicio especializado para gestionar la disponibilidad, horarios y bloqueos de los estilistas.
    /// </summary>
    public interface IEstilistaAgendaService
    {
        // CONSULTAS
        /// <summary>
        /// Obtiene la configuración de jornada laboral semanal (Horario Base).
        /// </summary>
        Task<IEnumerable<HorarioDiaDto>> GetHorarioBaseAsync(int estilistaId);

        /// <summary>
        /// Obtiene la lista de periodos de vacaciones o bloqueos especiales.
        /// </summary>
        Task<IEnumerable<BloqueoResponseDto>> GetBloqueosDiasLibresAsync(int estilistaId);

        /// <summary>
        /// Obtiene los descansos recurrentes (ej. hora de almuerzo).
        /// </summary>
        Task<IEnumerable<HorarioDiaDto>> GetDescansosFijosAsync(int estilistaId);

        // OPERACIONES

        /// <summary>
        /// Configura la jornada laboral estándar (ej. Lunes a Viernes 9am-6pm).
        /// </summary>
        Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios);

        /// <summary>
        /// Define descansos fijos dentro de la jornada laboral.
        /// </summary>
        Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansos);

        /// <summary>
        /// Elimina un descanso fijo de un día específico.
        /// </summary>
        Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia);

        /// <summary>
        /// Registra un periodo de ausencia (Vacaciones/Incapacidad).
        /// </summary>
        Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto);

        /// <summary>
        /// Modifica las fechas de un bloqueo existente.
        /// </summary>
        Task<bool> UpdateBloqueoDiasLibresAsync(int estilistaId, int bloqueoId, BloqueoRangoDto dto);

        /// <summary>
        /// Elimina un bloqueo, liberando la agenda para reservas.
        /// </summary>
        Task<bool> DeleteBloqueoDiasLibresAsync(int estilistaId, int bloqueoId);
    }
}