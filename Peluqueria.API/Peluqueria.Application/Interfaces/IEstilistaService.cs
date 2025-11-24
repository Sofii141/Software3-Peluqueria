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
        // CRUD
        Task<EstilistaDto> CreateAsync(CreateEstilistaRequestDto requestDto);
        Task<EstilistaDto?> UpdateAsync(int id, UpdateEstilistaRequestDto requestDto);
        Task<bool> InactivateAsync(int id);

        // CONSULTAS (AÑADIDOS)
        Task<IEnumerable<EstilistaDto>> GetAllAsync(); // PEL-HU-08
        Task<EstilistaDto?> GetByIdAsync(int id);

    }
}
