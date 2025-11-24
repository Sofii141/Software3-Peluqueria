using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.Application.Interfaces
{
    public interface IEstilistaAgendaService
    {
        // CONSULTAS
        Task<IEnumerable<HorarioDiaDto>> GetHorarioBaseAsync(int estilistaId);
        Task<IEnumerable<BloqueoRangoDto>> GetBloqueosDiasLibresAsync(int estilistaId);
        Task<IEnumerable<HorarioDiaDto>> GetDescansosFijosAsync(int estilistaId);

        // OPERACIONES - HORARIO
        Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios);

        // OPERACIONES - DESCANSOS
        Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansos);
        Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia); 

        // OPERACIONES - BLOQUEOS (VACACIONES)
        Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto);
        Task<bool> UpdateBloqueoDiasLibresAsync(int estilistaId, int bloqueoId, BloqueoRangoDto dto); 
        Task<bool> DeleteBloqueoDiasLibresAsync(int estilistaId, int bloqueoId);
    }
}