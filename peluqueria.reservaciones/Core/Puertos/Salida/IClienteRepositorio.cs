using peluqueria.reservaciones.Core.Dominio;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
    public interface IClienteRepositorio
    {
        Task SaveOrUpdateAsync(Cliente client);
        Task<Cliente?> GetByIdentificationAsync(string identification);
    }
}