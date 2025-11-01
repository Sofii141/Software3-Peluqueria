using Peluqueria.Application.Dtos.Categoria;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDto>> GetAllAsync();
    }
}