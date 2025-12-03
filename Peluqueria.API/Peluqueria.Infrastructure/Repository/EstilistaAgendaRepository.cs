using Microsoft.EntityFrameworkCore;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data;

namespace Peluqueria.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio especializado para gestionar las tablas satélites de la agenda (Horarios, Descansos, Bloqueos).
    /// </summary>
    public class EstilistaAgendaRepository : IEstilistaAgendaRepository
    {
        private readonly ApplicationDBContext _context;

        public EstilistaAgendaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Aplica lógica "Upsert" para los horarios base.
        /// </summary>
        /// <remarks>
        /// Si ya existe configuración para un día (ej. Lunes), la actualiza. 
        /// Si no existe, crea el registro nuevo.
        /// </remarks>
        public async Task UpdateHorarioBaseAsync(int estilistaId, List<HorarioSemanalBase> horariosEntrantes)
        {
            // 1. Traemos los horarios actuales de ese estilista para comparar
            var horariosExistentes = await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .ToListAsync();

            foreach (var nuevo in horariosEntrantes)
            {
                var existente = horariosExistentes.FirstOrDefault(h => h.DiaSemana == nuevo.DiaSemana);

                if (existente != null)
                {
                    // Actualizamos registro existente
                    existente.HoraInicioJornada = nuevo.HoraInicioJornada;
                    existente.HoraFinJornada = nuevo.HoraFinJornada;
                    existente.EsLaborable = nuevo.EsLaborable;
                    _context.HorariosSemanalBase.Update(existente);
                }
                else
                {
                    // Insertamos nuevo registro
                    await _context.HorariosSemanalBase.AddAsync(nuevo);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<HorarioSemanalBase>> GetHorarioBaseAsync(int estilistaId)
        {
            return await _context.HorariosSemanalBase
                .Where(h => h.EstilistaId == estilistaId)
                .OrderBy(h => h.DiaSemana) // Importante: Ordenar Lunes -> Domingo
                .ToListAsync();
        }

        /// <summary>
        /// Actualiza los descansos fijos aplicando una estrategia de "Borrar y Reemplazar".
        /// </summary>
        /// <remarks>
        /// Para los días afectados, elimina todos los descansos previos e inserta los nuevos.
        /// Esto evita tener que comparar uno por uno para detectar cambios de hora.
        /// </remarks>
        public async Task UpdateDescansosFijosAsync(int estilistaId, List<BloqueoDescansoFijoDiario> descansosEntrantes)
        {
            // 1. Identificar qué días de la semana estamos intentando actualizar
            var diasAfectados = descansosEntrantes.Select(x => x.DiaSemana).ToList();

            // 2. Buscar descansos viejos en esos días
            var descansosViejos = await _context.BloqueosDescansoFijoDiario
                .Where(d => d.EstilistaId == estilistaId && diasAfectados.Contains(d.DiaSemana))
                .ToListAsync();

            // 3. ELIMINAR los viejos
            if (descansosViejos.Any())
            {
                _context.BloqueosDescansoFijoDiario.RemoveRange(descansosViejos);
                await _context.SaveChangesAsync();
            }

            // 4. INSERTAR los nuevos
            if (descansosEntrantes.Any())
            {
                // Aseguramos ID=0 para que EF Core sepa que son inserciones
                foreach (var nuevo in descansosEntrantes) nuevo.Id = 0;

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

        // --- BLOQUEOS / VACACIONES ---

        public async Task<BloqueoRangoDiasLibres> CreateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo)
        {
            await _context.BloqueosRangoDiasLibres.AddAsync(bloqueo);
            await _context.SaveChangesAsync();
            return bloqueo;
        }

        public async Task<bool> UpdateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo)
        {
            var existing = await _context.BloqueosRangoDiasLibres.FindAsync(bloqueo.Id);

            // Validamos propiedad (seguridad básica)
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

        public async Task<IEnumerable<BloqueoRangoDiasLibres>> GetBloqueosDiasLibresAsync(int estilistaId)
        {
            return await _context.BloqueosRangoDiasLibres
                .Where(b => b.EstilistaId == estilistaId)
                .OrderByDescending(b => b.FechaInicioBloqueo) // Vacaciones más recientes primero
                .ToListAsync();
        }

        /// <summary>
        /// Consulta rápida para validar si un día está configurado como laborable.
        /// </summary>
        public async Task<bool> IsDiaLaborableAsync(int estilistaId, DayOfWeek dia)
        {
            var h = await _context.HorariosSemanalBase
                .FirstOrDefaultAsync(x => x.EstilistaId == estilistaId && x.DiaSemana == dia);

            return h != null && h.EsLaborable;
        }
    }
}