using Peluqueria.Application.Dtos.Categoria;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Servicio para la gestión de Categorías de servicios.
    /// </summary>
    public interface ICategoriaService
    {
        /// <summary>
        /// Obtiene todas las categorías registradas.
        /// </summary>
        Task<IEnumerable<CategoriaDto>> GetAllAsync();

        /// <summary>
        /// Crea una nueva categoría y notifica el evento a RabbitMQ.
        /// </summary>
        Task<CategoriaDto> CreateAsync(CreateCategoriaRequestDto requestDto); // PEL-HU-21

        /// <summary>
        /// Actualiza el nombre o estado de una categoría existente.
        /// </summary>
        Task<CategoriaDto?> UpdateAsync(int id, UpdateCategoriaRequestDto requestDto); // PEL-HU-22

        /// <summary>
        /// Realiza el borrado lógico de una categoría.
        /// </summary>
        /// <remarks>
        /// Valida con el microservicio de reservas si existen citas futuras antes de inactivar.
        /// </remarks>
        Task<bool> InactivateAsync(int id); // PEL-HU-23
    }
}