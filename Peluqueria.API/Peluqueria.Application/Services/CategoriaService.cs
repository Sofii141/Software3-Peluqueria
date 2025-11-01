using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Application.Services
{
    // Implementación de la capa intermedia para las Categorías.
    // Su trabajo es orquestar el repositorio y mapear la entidad de Dominio a DTO.
    public class CategoriaService : ICategoriaService
    {
        // Solo inyecta el repositorio, que es el único detalle de persistencia que necesita.
        private readonly ICategoriaRepository _categoriaRepo;

        public CategoriaService(ICategoriaRepository categoriaRepo)
        {
            _categoriaRepo = categoriaRepo;
        }

        public async Task<IEnumerable<CategoriaDto>> GetAllAsync()
        {
            var categorias = await _categoriaRepo.GetAllAsync();

            // Lógica de negocio: Mapeo de Entidad de Dominio (Categoria) a DTO de Salida (CategoriaDto)
            var categoriaDtos = categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre
            });

            return categoriaDtos;
        }
    }
}