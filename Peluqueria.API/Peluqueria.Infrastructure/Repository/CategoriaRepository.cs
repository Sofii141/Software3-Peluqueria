using Microsoft.EntityFrameworkCore;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data;

namespace Peluqueria.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación de acceso a datos para la entidad Categoría.
    /// </summary>
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDBContext _context;

        public CategoriaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categoria>> GetAllAsync()
        {
            // Retorna todas, incluso las inactivas, para gestión administrativa.
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

        /// <summary>
        /// Inserta una nueva categoría en la base de datos.
        /// </summary>
        public async Task<Categoria> CreateAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
            return categoria;
        }

        /// <summary>
        /// Actualiza los datos de la categoría. Permite reactivar categorías inactivas.
        /// </summary>
        public async Task<Categoria?> UpdateAsync(int id, Categoria categoria)
        {
            var existingCategoria = await _context.Categorias.FindAsync(id);
            if (existingCategoria == null) return null;

            existingCategoria.Nombre = categoria.Nombre;
            existingCategoria.EstaActiva = categoria.EstaActiva;

            await _context.SaveChangesAsync();
            return existingCategoria;
        }

        /// <summary>
        /// Ejecuta el Soft Delete (Baja Lógica).
        /// </summary>
        /// <remarks>
        /// No elimina el registro físicamente para mantener integridad referencial con el historial de servicios.
        /// </remarks>
        public async Task<bool> InactivateAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null) return false;

            categoria.EstaActiva = false;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}