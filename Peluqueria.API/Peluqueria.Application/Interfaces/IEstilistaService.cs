using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Servicio para la gestión de perfiles de Estilistas.
    /// </summary>
    public interface IEstilistaService
    {
        /// <summary>
        /// Crea un nuevo estilista, su usuario de Identity asociado y sus especialidades.
        /// </summary>
        Task<EstilistaDto> CreateAsync(CreateEstilistaRequestDto requestDto);

        /// <summary>
        /// Actualiza datos personales, credenciales o servicios de un estilista.
        /// </summary>
        Task<EstilistaDto?> UpdateAsync(int id, UpdateEstilistaRequestDto requestDto);

        /// <summary>
        /// Inactiva a un estilista (Baja lógica).
        /// </summary>
        /// <remarks>
        /// Verifica en el microservicio si tiene agenda pendiente antes de proceder.
        /// </remarks>
        Task<bool> InactivateAsync(int id);

        /// <summary>
        /// Obtiene el detalle de un estilista por ID.
        /// </summary>
        Task<EstilistaDto> GetByIdAsync(int id);

        /// <summary>
        /// Lista todos los estilistas activos e inactivos (para admin).
        /// </summary>
        Task<IEnumerable<EstilistaDto>> GetAllAsync();
    }
}