using Peluqueria.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IServicioRepository
    {
        Task<IEnumerable<Servicio>> GetAllAsync();
        Task<Servicio?> GetByIdAsync(int id);
        Task<IEnumerable<Servicio>> GetByCategoriaIdAsync(int categoriaId);
        Task<Servicio> CreateAsync(Servicio servicio);
        Task<Servicio?> UpdateAsync(int id, Servicio servicio);
        Task<bool> DeleteAsync(int id);
    }
}