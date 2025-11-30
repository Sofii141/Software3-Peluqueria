using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.Persistencia;

namespace peluqueria.reservaciones.Infraestructura.Repositorios
{
    public class CategoriaRepositorio : ICategoriaRepositorio
    {
        private readonly ReservacionesDbContext _context;

        public CategoriaRepositorio(ReservacionesDbContext context)
        {
            _context = context;
        }

        public async Task<Categoria?> GetByIdAsync(int categoryId)
        {
            return await _context.Categorias.FindAsync(categoryId);
        }

        public async Task SaveOrUpdateAsync(Categoria category)
        {
            var existing = await GetByIdAsync(category.Id);

            if (existing == null)
            {
                _context.Categorias.Add(category);
            }
            else
            {
                existing.Nombre = category.Nombre;
                existing.EstaActiva = category.EstaActiva;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int categoryId)
        {
            var existing = await GetByIdAsync(categoryId);
            if (existing != null)
            {
                existing.EstaActiva = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}