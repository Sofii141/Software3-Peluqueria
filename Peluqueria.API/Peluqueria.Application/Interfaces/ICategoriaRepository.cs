using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Repositorio para el acceso a datos de la entidad Categoria.
    /// </summary>
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(int id);

        /// <summary>
        /// Persiste una nueva categoría en la BD.
        /// </summary>
        Task<Categoria> CreateAsync(Categoria categoria);

        Task<Categoria?> UpdateAsync(int id, Categoria categoria);

        /// <summary>
        /// Marca la categoría como inactiva (Soft Delete).
        /// </summary>
        Task<bool> InactivateAsync(int id);

        Task<Categoria?> GetByNameAsync(string nombre);
    }
}