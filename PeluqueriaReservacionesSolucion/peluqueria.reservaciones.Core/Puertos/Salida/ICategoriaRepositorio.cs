using peluqueria.reservaciones.Core.Dominio;
using System.Threading.Tasks;

/*
 @author: Juan David Moran
	@description: Interfaz para el repositorio de categorías.
 */

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
	public interface ICategoriaRepositorio
	{
		Task SaveOrUpdateAsync(Categoria category);
		Task DeactivateAsync(int categoryId);
		Task<Categoria?> GetByIdAsync(int categoryId);
	}
}