using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.Persistencia;

namespace peluqueria.reservaciones.Infraestructura.Repositorios
{
    public class ServicioRepositorio : IServicioRepositorio
    {
        private readonly ReservacionesDbContext _context;

        public ServicioRepositorio(ReservacionesDbContext context)
        {
            _context = context;
        }

        public async Task<Servicio?> GetByIdAsync(int serviceId)
        {
            return await _context.Servicios.FindAsync(serviceId);
        }

        public async Task SaveOrUpdateAsync(Servicio service)
        {
            var existing = await GetByIdAsync(service.Id);

            if (existing == null)
            {
                _context.Servicios.Add(service);
            }
            else
            {
                
                existing.Nombre = service.Nombre;
                existing.DuracionMinutos = service.DuracionMinutos;
                existing.Precio = service.Precio;
                existing.Disponible = service.Disponible;
                existing.CategoriaId = service.CategoriaId; 
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int serviceId)
        {
            var existing = await GetByIdAsync(serviceId);
            if (existing != null)
            {
                existing.Disponible = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}