using peluqueria.reservaciones.Core.Dominio;
using System.Threading.Tasks;

/*
 @author: Juan David Moran
	@description: Interfaz para el repositorio de estilista.
 */

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
    public interface IEstilistaRepositorio
    {
        Task SaveOrUpdateAsync(Estilista stylist);
        Task DeactivateAsync(int stylistId);
        Task<Estilista?> GetByIdAsync(int stylistId);
    }
}