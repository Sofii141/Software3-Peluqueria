using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepo;

        public CategoriaService(ICategoriaRepository categoriaRepo)
        {
            _categoriaRepo = categoriaRepo;
        }

        public async Task<IEnumerable<CategoriaDto>> GetAllAsync()
        {
            var categorias = await _categoriaRepo.GetAllAsync();

            var categoriaDtos = categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre
            });

            return categoriaDtos;
        }
    }
}