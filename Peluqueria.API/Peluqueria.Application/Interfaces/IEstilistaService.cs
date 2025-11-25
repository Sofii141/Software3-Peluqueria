using Peluqueria.Application.Dtos.Estilista;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IEstilistaService
    {
        Task<EstilistaDto> CreateAsync(CreateEstilistaRequestDto requestDto);
        Task<EstilistaDto?> UpdateAsync(int id, UpdateEstilistaRequestDto requestDto);
        Task<bool> InactivateAsync(int id);
        Task<EstilistaDto> GetByIdAsync(int id);
        Task<IEnumerable<EstilistaDto>> GetAllAsync(); 

    }
}
