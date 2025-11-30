using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.Persistencia;
using System.Collections.Generic;

namespace peluqueria.reservaciones.Infraestructura.Repositorios
{
    public class EstilistaRepositorio : IEstilistaRepositorio
    {
        private readonly ReservacionesDbContext _context;

        public EstilistaRepositorio(ReservacionesDbContext context)
        {
            _context = context;
        }

        public async Task<Estilista?> GetByIdAsync(int stylistId)
        {
            return await _context.Estilistas.FindAsync(stylistId);
        }

        public async Task SaveOrUpdateAsync(Estilista stylist)
        {
            var existing = await GetByIdAsync(stylist.Id);

            if (existing == null)
            {
                _context.Estilistas.Add(stylist);

                var horarioBase = new HorarioBase
                {
                    EstilistaId = stylist.Id,
                    HorariosSemanales = new List<DiaHorario>() 
                };

                var descansoFijo = new DescansoFijo
                {
                    EstilistaId = stylist.Id,
                    DescansosFijos = new List<DiaHorario>() 
                };

                _context.Horarios.Add(horarioBase);
                _context.DescansoFijo.Add(descansoFijo);
            }
            else
            {
                
                existing.NombreCompleto = stylist.NombreCompleto;
                existing.EstaActivo = stylist.EstaActivo;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int stylistId)
        {
            var existing = await GetByIdAsync(stylistId);
            if (existing != null)
            {
                existing.EstaActivo = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}