using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.Persistencia;

namespace peluqueria.reservaciones.Infraestructura.Repositorios
{
    public class HorarioRepositorio : IHorarioRepositorio
    {
        private readonly ReservacionesDbContext _context;

        public HorarioRepositorio(ReservacionesDbContext context)
        {
            _context = context;
        }

        // Obtiene HorarioBase e incluye los Días/Horarios anidados (Owned Types)
        public async Task<HorarioBase?> GetStylistScheduleAsync(int stylistId)
        {
            return await _context.Horarios
                .Include(h => h.HorariosSemanales) // Incluir la lista de Owned Types
                .FirstOrDefaultAsync(h => h.EstilistaId == stylistId);
        }

        public async Task SetBaseScheduleAsync(int stylistId, List<DiaHorario> schedules)
        {
            var existing = await GetStylistScheduleAsync(stylistId);

            if (existing == null)
            {
                // Creado
                var newSchedule = new HorarioBase { EstilistaId = stylistId, HorariosSemanales = schedules };
                _context.Horarios.Add(newSchedule);
            }
            else
            {
                // Actualizado: EF Core maneja la colección (Owned Types) automáticamente
                // Reemplazamos toda la colección, y EF Core eliminará los viejos e insertará los nuevos
                existing.HorariosSemanales = schedules;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetFixedBreaksAsync(int stylistId, List<DiaHorario> fixedBreaks)
        {
            var existing = await GetDescanso(stylistId);

            if (existing == null)
            {
                // Creado
                var newBreak = new DescansoFijo { EstilistaId = stylistId, DescansosFijos = fixedBreaks };
                _context.DescansoFijo.Add(newBreak);
            }
            else
            {
                existing.DescansosFijos = fixedBreaks;
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddBlockoutRangeAsync(BloqueoRangoDiasLibres blockout)
        {
            // Para BloqueoRangoDiasLibres, el evento es Add (ya que no se envía la acción en la interfaz)
            // Basándonos en la interfaz, solo podemos Añadir. Si el consumer usa Delete, se debe actualizar la interfaz.
            // Para simplicidad, si ya existe la combinación (EstilistaId, FechaInicioBloqueo), asumiremos que se debe actualizar o ignorar.

            // Usando Attach para actualizar o Add para insertar
            _context.BloqueoRangoDias.Add(blockout);

            // El SaveChanges lanzará una excepción si el bloque ya existe (debido a la clave compuesta), 
            // a menos que primero se verifique su existencia.
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Ignoramos duplicados si el bloque ya existía con la misma clave compuesta
                Console.WriteLine($"Error al añadir bloqueo (posible duplicado o manejo de Delete pendiente): {ex.Message}");
            }
        }

        public async Task<BloqueoRangoDiasLibres?> GetRangoDiasLibres(int stylistId)
        {

            return await _context.BloqueoRangoDias
                .FirstOrDefaultAsync(b => b.EstilistaId == stylistId);
        }


        public async Task<DescansoFijo?> GetDescanso(int stylistId)
        {
            return await _context.DescansoFijo
                .Include(d => d.DescansosFijos)
                .FirstOrDefaultAsync(d => d.EstilistaId == stylistId);
        }
    }
}