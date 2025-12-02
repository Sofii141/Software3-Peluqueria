using peluqueria.reservaciones.Core.Dominio;
using System.Threading.Tasks;

/*
 @author: Juan David Moran
	@description: Interfaz para el repositorio de clientes.
 */

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
    public interface IClienteRepositorio
    {
        Task SaveOrUpdateAsync(Cliente client);
        Task<Cliente?> GetByIdentificationAsync(string identification);
    }
}