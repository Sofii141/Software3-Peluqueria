using peluqueria.reservaciones.Core.Dominio;
using System.Threading.Tasks;

/*
 @author: Juan David Moran
	@description: Interfaz para el repositorio de servicios.
 */

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
    public interface IServicioRepositorio
    {
        Task SaveOrUpdateAsync(Servicio service);
        Task DeactivateAsync(int serviceId);
        Task<Servicio?> GetByIdAsync(int serviceId);
    }
}