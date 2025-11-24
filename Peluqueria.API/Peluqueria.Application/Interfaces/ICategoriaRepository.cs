using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(int id);
        Task<Categoria> CreateAsync(Categoria categoria); // Para crear (PEL-HU-21)
        Task<Categoria?> UpdateAsync(int id, Categoria categoria); // Para actualizar (PEL-HU-22)
        Task<bool> InactivateAsync(int id); // Para baja lógica (PEL-HU-23)
        Task<Categoria?> GetByNameAsync(string nombre); // Para validación de unicidad (G-ERROR-007)
    }
}