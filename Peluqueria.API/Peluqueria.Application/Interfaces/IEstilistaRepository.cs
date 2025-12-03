using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Repositorio para gestionar datos del Estilista y sus relaciones.
    /// </summary>
    public interface IEstilistaRepository
    {
        /// <summary>
        /// Crea el estilista y sus relaciones en la tabla intermedia (EstilistaServicio).
        /// </summary>
        Task<Estilista> CreateAsync(Estilista estilista, List<int> serviciosIds);

        Task<Estilista?> UpdateAsync(Estilista estilista, List<int> serviciosIds);

        /// <summary>
        /// Obtiene un estilista con todas sus relaciones cargadas (Servicios, Horarios, Bloqueos).
        /// </summary>
        Task<Estilista?> GetFullEstilistaByIdAsync(int id);

        Task<IEnumerable<Estilista>> GetAllAsync();

        Task<Estilista?> GetByIdentityIdAsync(string identityId);
    }
}