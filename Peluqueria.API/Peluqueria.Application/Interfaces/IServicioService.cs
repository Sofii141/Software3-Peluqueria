using Peluqueria.Application.Dtos.Servicio;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Servicio para la gestión del catálogo de servicios (Cortes, Manicura, etc.).
    /// </summary>
    public interface IServicioService
    {
        /// <summary>
        /// Obtiene el catálogo completo de servicios.
        /// </summary>
        Task<IEnumerable<ServicioDto>> GetAllAsync();

        /// <summary>
        /// Busca un servicio por su ID.
        /// </summary>
        Task<ServicioDto?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene los servicios filtrados por categoría.
        /// </summary>
        Task<IEnumerable<ServicioDto>> GetByCategoriaIdAsync(int categoriaId);

        /// <summary>
        /// Crea un servicio, sube su imagen y valida reglas de negocio (Duración, Precio).
        /// </summary>
        Task<ServicioDto> CreateAsync(CreateServicioRequestDto requestDto);

        /// <summary>
        /// Actualiza la información de un servicio.
        /// </summary>
        Task<ServicioDto?> UpdateAsync(int id, UpdateServicioRequestDto requestDto);

        /// <summary>
        /// Desactiva un servicio para que no pueda ser reservado.
        /// </summary>
        /// <remarks>
        /// Lanza excepción si existen reservas futuras asociadas en el microservicio.
        /// </remarks>
        Task<bool> InactivateAsync(int id);
    }
}