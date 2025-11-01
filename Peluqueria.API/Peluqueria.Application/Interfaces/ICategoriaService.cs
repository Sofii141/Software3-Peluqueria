using Peluqueria.Application.Dtos.Categoria;

namespace Peluqueria.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDto>> GetAllAsync();
    }
}