using Peluqueria.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync();
    }
}