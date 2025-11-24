using Peluqueria.Application.Dtos.Categoria;

namespace Peluqueria.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDto>> GetAllAsync();
        Task<CategoriaDto> CreateAsync(CreateCategoriaRequestDto requestDto); // PEL-HU-21
        Task<CategoriaDto?> UpdateAsync(int id, UpdateCategoriaRequestDto requestDto); // PEL-HU-22
        Task<bool> InactivateAsync(int id); // PEL-HU-23
    }
}