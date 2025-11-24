using Peluqueria.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IEstilistaAgendaRepository
    {
        // HORARIO BASE
        Task UpdateHorarioBaseAsync(int estilistaId, List<HorarioSemanalBase> horarios);
        Task<IEnumerable<HorarioSemanalBase>> GetHorarioBaseAsync(int estilistaId);

        // DESCANSOS
        Task UpdateDescansosFijosAsync(int estilistaId, List<BloqueoDescansoFijoDiario> descansos);
        Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia); 
        Task<IEnumerable<BloqueoDescansoFijoDiario>> GetDescansosFijosAsync(int estilistaId);

        // BLOQUEOS (VACACIONES)
        Task<BloqueoRangoDiasLibres> CreateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo);
        Task<bool> UpdateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo);
        Task<bool> DeleteBloqueoDiasLibresAsync(int id, int estilistaId); 
        Task<IEnumerable<BloqueoRangoDiasLibres>> GetBloqueosDiasLibresAsync(int estilistaId);

        // HELPER VALIDACIÓN
        Task<bool> IsDiaLaborableAsync(int estilistaId, DayOfWeek dia);
    }
}