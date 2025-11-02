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
    }
}
