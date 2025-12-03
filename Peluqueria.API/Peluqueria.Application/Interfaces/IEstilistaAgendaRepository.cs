using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Repositorio especializado para las tablas de agenda (HorarioBase, Descansos, Bloqueos).
    /// </summary>
    public interface IEstilistaAgendaRepository
    {
        // HORARIO BASE
        /// <summary>
        /// Actualiza o crea la configuración de horario semanal.
        /// </summary>
        Task UpdateHorarioBaseAsync(int estilistaId, List<HorarioSemanalBase> horarios);
        Task<IEnumerable<HorarioSemanalBase>> GetHorarioBaseAsync(int estilistaId);

        // DESCANSOS
        /// <summary>
        /// Reemplaza los descansos fijos de los días especificados.
        /// </summary>
        Task UpdateDescansosFijosAsync(int estilistaId, List<BloqueoDescansoFijoDiario> descansos);
        Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia);
        Task<IEnumerable<BloqueoDescansoFijoDiario>> GetDescansosFijosAsync(int estilistaId);

        // BLOQUEOS (VACACIONES)
        Task<BloqueoRangoDiasLibres> CreateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo);
        Task<bool> UpdateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo);
        Task<bool> DeleteBloqueoDiasLibresAsync(int id, int estilistaId);
        Task<IEnumerable<BloqueoRangoDiasLibres>> GetBloqueosDiasLibresAsync(int estilistaId);

        // HELPER
        /// <summary>
        /// Verifica si un estilista tiene configurado un día como laborable.
        /// </summary>
        Task<bool> IsDiaLaborableAsync(int estilistaId, DayOfWeek dia);
    }
}