using Peluqueria.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IEstilistaRepository
    {
        // CRUD de Estilista
        Task<Estilista> CreateAsync(Estilista estilista, List<int> serviciosIds);
        Task<Estilista?> UpdateAsync(Estilista estilista, List<int> serviciosIds);
        Task<Estilista?> GetFullEstilistaByIdAsync(int id); // Ya existe, usado por servicios
        Task<IEnumerable<Estilista>> GetAllAsync(); // PEL-HU-08 (AÑADIDO)

        // Búsqueda por credencial (IdentityId) para mapear con Identity
        Task<Estilista?> GetByIdentityIdAsync(string identityId); // Ya existe
    }
}
