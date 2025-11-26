using peluqueria.reservaciones.Core.Dominio;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
	public interface ICategoriaRepositorio
	{
		Task SaveOrUpdateAsync(Categoria category);
		Task DeactivateAsync(int categoryId);
		Task<Categoria?> GetByIdAsync(int categoryId);
	}
}