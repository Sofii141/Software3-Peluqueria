using Microsoft.EntityFrameworkCore;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data;

namespace Peluqueria.Infrastructure.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly ApplicationDBContext _context;
        public ServicioRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Servicio>> GetAllAsync()
        {
            return await _context.Servicios.Include(s => s.Categoria).ToListAsync();
        }

        public async Task<Servicio?> GetByIdAsync(int id)
        {
            return await _context.Servicios.Include(s => s.Categoria).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Servicio>> GetByCategoriaIdAsync(int categoriaId)
        {
            return await _context.Servicios
                .Include(s => s.Categoria)
                .Where(s => s.CategoriaId == categoriaId)
                .ToListAsync();
        }

        public async Task<Servicio> CreateAsync(Servicio servicio)
        {
            await _context.Servicios.AddAsync(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<Servicio?> UpdateAsync(int id, Servicio servicio)
        {
            var existingServicio = await _context.Servicios.FindAsync(id);
            if (existingServicio == null) return null;

            existingServicio.Nombre = servicio.Nombre;
            existingServicio.Descripcion = servicio.Descripcion;
            existingServicio.Precio = servicio.Precio;
            existingServicio.Disponible = servicio.Disponible;
            existingServicio.CategoriaId = servicio.CategoriaId;

            if (!string.IsNullOrEmpty(servicio.Imagen))
            {
                existingServicio.Imagen = servicio.Imagen;
            }

            await _context.SaveChangesAsync();
            return existingServicio;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            
            if (servicio == null) return false;

            _context.Servicios.Remove(servicio);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}