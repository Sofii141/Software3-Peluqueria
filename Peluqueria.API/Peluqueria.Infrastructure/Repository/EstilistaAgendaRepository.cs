using Microsoft.EntityFrameworkCore;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data; // Asegúrate de que este using apunte a tu DbContext

namespace Peluqueria.Infrastructure.Repositories
{
    public class EstilistaAgendaRepository : IEstilistaAgendaRepository
    {
        private readonly ApplicationDBContext _context;

        public EstilistaAgendaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        // --- HORARIO BASE (Lógica Upsert: Actualizar si existe, Crear si no) ---
        public async Task UpdateHorarioBaseAsync(int estilistaId, List<HorarioSemanalBase> horariosEntrantes)
        {
            // 1. Traemos los horarios actuales de ese estilista
            var horariosExistentes = await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .ToListAsync();

            foreach (var nuevo in horariosEntrantes)
            {
                // Buscamos si ya existe configuración para ese día (Lunes, Martes...)
                var existente = horariosExistentes.FirstOrDefault(h => h.DiaSemana == nuevo.DiaSemana);

                if (existente != null)
                {
                    // Actualizamos
                    existente.HoraInicioJornada = nuevo.HoraInicioJornada;
                    existente.HoraFinJornada = nuevo.HoraFinJornada;
                    existente.EsLaborable = nuevo.EsLaborable;
                    _context.HorariosSemanalBase.Update(existente);
                }
                else
                {
                    // Creamos
                    await _context.HorariosSemanalBase.AddAsync(nuevo);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<HorarioSemanalBase>> GetHorarioBaseAsync(int estilistaId)
        {
            return await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .OrderBy(h => h.DiaSemana) // Ordenar Lunes -> Domingo
                .ToListAsync();
        }

        public async Task UpdateDescansosFijosAsync(int estilistaId, List<BloqueoDescansoFijoDiario> descansosEntrantes)
        {
            // 1. Identificar qué días de la semana estamos intentando actualizar (ej: Lunes)
            var diasAfectados = descansosEntrantes.Select(x => x.DiaSemana).ToList();

            // 2. Buscar en la BD si ya existen descansos para esos días específicos y ese estilista
            var descansosViejos = await _context.BloqueosDescansoFijoDiario
                .Where(d => d.EstilistaId == estilistaId && diasAfectados.Contains(d.DiaSemana))
                .ToListAsync();

            // 3. ELIMINAR los viejos (Limpiamos el camino)
            if (descansosViejos.Any())
            {
                _context.BloqueosDescansoFijoDiario.RemoveRange(descansosViejos);
                // Guardamos cambios aquí para confirmar el borrado antes de insertar
                await _context.SaveChangesAsync();
            }

            // 4. INSERTAR los nuevos
            if (descansosEntrantes.Any())
            {
                // Truco de seguridad: Aseguramos que el ID sea 0 para que EF entienda que es nuevo
                foreach (var nuevo in descansosEntrantes)
                {
                    nuevo.Id = 0;
                }

                await _context.BloqueosDescansoFijoDiario.AddRangeAsync(descansosEntrantes);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BloqueoDescansoFijoDiario>> GetDescansosFijosAsync(int estilistaId)
        {
            return await _context.BloqueosDescansoFijoDiario
                .Where(d => d.EstilistaId == estilistaId)
                .OrderBy(d => d.DiaSemana)
                .ToListAsync();
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

        // --- BLOQUEOS / VACACIONES (CRUD Estándar) ---
        public async Task<BloqueoRangoDiasLibres> CreateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo)
        {
            await _context.BloqueosRangoDiasLibres.AddAsync(bloqueo);
            await _context.SaveChangesAsync();
            return bloqueo;
        }

        public async Task<bool> UpdateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo)
        {
            var existing = await _context.BloqueosRangoDiasLibres.FindAsync(bloqueo.Id);

            // Validamos que exista y que pertenezca al estilista correcto (Seguridad)
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

            // Validamos que exista y que pertenezca al estilista correcto
            if (existing == null || existing.EstilistaId != estilistaId) return false;

            _context.BloqueosRangoDiasLibres.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BloqueoRangoDiasLibres>> GetBloqueosDiasLibresAsync(int estilistaId)
        {
            return await _context.BloqueosRangoDiasLibres
                .Where(b => b.EstilistaId == estilistaId)
                .OrderByDescending(b => b.FechaInicioBloqueo) // Los más recientes primero
                .ToListAsync();
        }

        // --- HELPER VALIDACIÓN ---
        public async Task<bool> IsDiaLaborableAsync(int estilistaId, DayOfWeek dia)
        {
            // Consultamos si existe configuración para ese día
            var h = await _context.HorariosSemanalBase
                .FirstOrDefaultAsync(x => x.EstilistaId == estilistaId && x.DiaSemana == dia);

            // Retorna true SOLO si existe Y está marcado como laborable
            return h != null && h.EsLaborable;
        }
    }
}