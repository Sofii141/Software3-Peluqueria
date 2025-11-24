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

        // --- HORARIO BASE (UPSERT) ---
        public async Task UpdateHorarioBaseAsync(int estilistaId, List<HorarioSemanalBase> horariosEntrantes)
        {
            var horariosExistentes = await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .ToListAsync();

            foreach (var nuevo in horariosEntrantes)
            {
                var existente = horariosExistentes.FirstOrDefault(h => h.DiaSemana == nuevo.DiaSemana);
                if (existente != null)
                {
                    // Actualizar
                    existente.HoraInicioJornada = nuevo.HoraInicioJornada;
                    existente.HoraFinJornada = nuevo.HoraFinJornada;
                    existente.EsLaborable = nuevo.EsLaborable;
                    _context.HorariosSemanalBase.Update(existente);
                }
                else
                {
                    // Crear
                    await _context.HorariosSemanalBase.AddAsync(nuevo);
                }
            }
            await _context.SaveChangesAsync();
        }

        // --- DESCANSOS FIJOS (UPSERT) ---
        public async Task UpdateDescansosFijosAsync(int estilistaId, List<BloqueoDescansoFijoDiario> descansosEntrantes)
        {
            var descansosExistentes = await _context.BloqueosDescansoFijoDiario
                .Where(d => d.EstilistaId == estilistaId)
                .ToListAsync();

            foreach (var nuevo in descansosEntrantes)
            {
                var existente = descansosExistentes.FirstOrDefault(d => d.DiaSemana == nuevo.DiaSemana);
                if (existente != null)
                {
                    existente.HoraInicioDescanso = nuevo.HoraInicioDescanso;
                    existente.HoraFinDescanso = nuevo.HoraFinDescanso;
                    _context.BloqueosDescansoFijoDiario.Update(existente);
                }
                else
                {
                    await _context.BloqueosDescansoFijoDiario.AddAsync(nuevo);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia)
        {
            var item = await _context.BloqueosDescansoFijoDiario
                .FirstOrDefaultAsync(d => d.EstilistaId == estilistaId && d.DiaSemana == dia);
            if (item != null)
            {
                _context.BloqueosDescansoFijoDiario.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // --- BLOQUEOS (CRUD) ---
        public async Task<BloqueoRangoDiasLibres> CreateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo)
        {
            await _context.BloqueosRangoDiasLibres.AddAsync(bloqueo);
            await _context.SaveChangesAsync();
            return bloqueo;
        }

        public async Task<bool> UpdateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo)
        {
            var existing = await _context.BloqueosRangoDiasLibres.FindAsync(bloqueo.Id);
            if (existing == null || existing.EstilistaId != bloqueo.EstilistaId) return false;

            existing.FechaInicioBloqueo = bloqueo.FechaInicioBloqueo;
            existing.FechaFinBloqueo = bloqueo.FechaFinBloqueo;
            existing.Razon = bloqueo.Razon;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBloqueoDiasLibresAsync(int id, int estilistaId)
        {
            var existing = await _context.BloqueosRangoDiasLibres.FindAsync(id);
            if (existing == null || existing.EstilistaId != estilistaId) return false;

            _context.BloqueosRangoDiasLibres.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- HELPERS Y GETTERS ---
        public async Task<bool> IsDiaLaborableAsync(int estilistaId, DayOfWeek dia)
        {
            var h = await _context.HorariosSemanalBase
                .FirstOrDefaultAsync(x => x.EstilistaId == estilistaId && x.DiaSemana == dia);
            return h != null && h.EsLaborable;
        }

        public async Task<IEnumerable<HorarioSemanalBase>> GetHorarioBaseAsync(int estilistaId)
        {
            return await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .OrderBy(h => h.DiaSemana)
                .ToListAsync();
        }

        public async Task<IEnumerable<BloqueoDescansoFijoDiario>> GetDescansosFijosAsync(int estilistaId)
        {
            return await _context.BloqueosDescansoFijoDiario.Where(d => d.EstilistaId == estilistaId).ToListAsync();
        }

        public async Task<IEnumerable<BloqueoRangoDiasLibres>> GetBloqueosDiasLibresAsync(int estilistaId)
        {
            return await _context.BloqueosRangoDiasLibres
                .Where(b => b.EstilistaId == estilistaId)
                .OrderByDescending(b => b.FechaInicioBloqueo)
                .ToListAsync();
        }
    }
}