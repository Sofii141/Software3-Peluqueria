using Peluqueria.Application.Dtos.Servicio;

namespace Peluqueria.Application.Interfaces
{
    public interface IServicioService
    {
        Task<IEnumerable<ServicioDto>> GetAllAsync();
        Task<ServicioDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServicioDto>> GetByCategoriaIdAsync(int categoriaId);
        Task<ServicioDto> CreateAsync(CreateServicioRequestDto requestDto);
        Task<ServicioDto?> UpdateAsync(int id, UpdateServicioRequestDto requestDto);
        Task<bool> InactivateAsync(int id);
    }
}