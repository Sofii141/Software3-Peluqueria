using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.Application.Interfaces
{
    public interface IEstilistaAgendaService
    {
        // CONSULTAS
        Task<IEnumerable<HorarioDiaDto>> GetHorarioBaseAsync(int estilistaId);

        // CORRECCIÓN: Ahora devuelve ResponseDto (Con ID)
        Task<IEnumerable<BloqueoResponseDto>> GetBloqueosDiasLibresAsync(int estilistaId);

        Task<IEnumerable<HorarioDiaDto>> GetDescansosFijosAsync(int estilistaId);

        // OPERACIONES
        Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios);
        Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansos);
        Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia);
        Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto);
        Task<bool> UpdateBloqueoDiasLibresAsync(int estilistaId, int bloqueoId, BloqueoRangoDto dto);
        Task<bool> DeleteBloqueoDiasLibresAsync(int estilistaId, int bloqueoId);
    }
}