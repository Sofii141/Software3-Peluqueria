// Peluqueria.Application/Interfaces/IServicioService.cs
using Peluqueria.Application.Dtos.Servicio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IServicioService
    {
        Task<IEnumerable<ServicioDto>> GetAllAsync();
        Task<ServicioDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServicioDto>> GetByCategoriaIdAsync(int categoriaId);
        Task<ServicioDto> CreateAsync(CreateServicioRequestDto requestDto);
        Task<ServicioDto?> UpdateAsync(int id, UpdateServicioRequestDto requestDto);
        Task<bool> DeleteAsync(int id);
    }
}