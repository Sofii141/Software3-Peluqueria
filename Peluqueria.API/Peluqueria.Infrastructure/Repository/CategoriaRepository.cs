using Microsoft.EntityFrameworkCore;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data;

namespace Peluqueria.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDBContext _context;

        public CategoriaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categoria>> GetAllAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<Categoria?> GetByIdAsync(int id)
        {
            return await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Categoria?> GetByNameAsync(string nombre)
        {
            return await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());
        }

        // PEL-HU-21: Crear
        public async Task<Categoria> CreateAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
            return categoria;
        }

        // PEL-HU-22: Actualizar
        public async Task<Categoria?> UpdateAsync(int id, Categoria categoria)
        {
            var existingCategoria = await _context.Categorias.FindAsync(id);
            if (existingCategoria == null) return null;

            existingCategoria.Nombre = categoria.Nombre;
            existingCategoria.EstaActiva = categoria.EstaActiva; // Permite reactivación

            await _context.SaveChangesAsync();
            return existingCategoria;
        }

        // PEL-HU-23: Inactivar (Baja Lógica)
        public async Task<bool> InactivateAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null) return false;

            // Bloqueo: si tiene servicios asociados, el servicio de negocio lo validará,
            // pero el repositorio solo aplica la baja lógica
            categoria.EstaActiva = false;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}