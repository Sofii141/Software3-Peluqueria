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

        public async Task<HorarioBase?> GetStylistScheduleAsync(int stylistId)
        {
            return await _context.Horarios
                .Include(h => h.HorariosSemanales) 
                .FirstOrDefaultAsync(h => h.EstilistaId == stylistId);
        }

        public async Task SetBaseScheduleAsync(int stylistId, List<DiaHorario> schedules)
        {
            var existing = await GetStylistScheduleAsync(stylistId);

            if (existing == null)
            {
                var newSchedule = new HorarioBase { EstilistaId = stylistId, HorariosSemanales = schedules };
                _context.Horarios.Add(newSchedule);
            }
            else
            {
                existing.HorariosSemanales = schedules;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetFixedBreaksAsync(int stylistId, List<DiaHorario> fixedBreaks)
        {
            var existing = await GetDescanso(stylistId);

            if (existing == null)
            {
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

            _context.BloqueoRangoDias.Add(blockout);

 
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
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