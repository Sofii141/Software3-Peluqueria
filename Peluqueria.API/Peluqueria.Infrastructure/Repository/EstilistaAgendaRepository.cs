using Microsoft.EntityFrameworkCore;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data;

namespace Peluqueria.Infrastructure.Repository
{
    public class EstilistaAgendaRepository : IEstilistaAgendaRepository 
    {
        private readonly ApplicationDBContext _context;

        public EstilistaAgendaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        // Implementación de Horario Base (Movido de EstilistaRepository)
        public async Task UpdateHorarioBaseAsync(int estilistaId, List<HorarioSemanalBase> horarios)
        {
            // 1. Eliminar todos los horarios existentes para el estilista
            var existingHorarios = await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .ToListAsync();

            _context.HorariosSemanalBase.RemoveRange(existingHorarios);

            // 2. Agregar los nuevos horarios (insertar)
            await _context.HorariosSemanalBase.AddRangeAsync(horarios);

            await _context.SaveChangesAsync();
        }

        // Implementación de Bloqueo Rango Días Libres (Movido de EstilistaRepository)
        public async Task<BloqueoRangoDiasLibres> CreateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo)
        {
            await _context.BloqueosRangoDiasLibres.AddAsync(bloqueo);
            await _context.SaveChangesAsync();
            return bloqueo;
        }

        // Implementación de Descansos Fijos (Movido de EstilistaRepository)
        public async Task UpdateDescansosFijosAsync(int estilistaId, List<BloqueoDescansoFijoDiario> descansos)
        {
            // 1. Eliminar todos los descansos fijos existentes para el estilista
            var existingDescansos = await _context.BloqueosDescansoFijoDiario
                .Where(d => d.EstilistaId == estilistaId)
                .ToListAsync();

            _context.BloqueosDescansoFijoDiario.RemoveRange(existingDescansos);

            // 2. Agregar los nuevos descansos (insertar)
            await _context.BloqueosDescansoFijoDiario.AddRangeAsync(descansos);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<HorarioSemanalBase>> GetHorarioBaseAsync(int estilistaId)
        {
            return await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .ToListAsync();
        }

        // Implementación de Get Bloqueos Días Libres
        public async Task<IEnumerable<BloqueoRangoDiasLibres>> GetBloqueosDiasLibresAsync(int estilistaId)
        {
            // Solo mostramos los futuros o los que están vigentes
            return await _context.BloqueosRangoDiasLibres
                .Where(b => b.EstilistaId == estilistaId && b.FechaFinBloqueo.Date >= DateTime.Today)
                .ToListAsync();
        }

        // Implementación de Get Descansos Fijos
        public async Task<IEnumerable<BloqueoDescansoFijoDiario>> GetDescansosFijosAsync(int estilistaId)
        {
            return await _context.BloqueosDescansoFijoDiario
                .Where(d => d.EstilistaId == estilistaId)
                .ToListAsync();
        }
    }

}
