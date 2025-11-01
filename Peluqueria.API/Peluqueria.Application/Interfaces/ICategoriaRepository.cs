using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync();
    }
}