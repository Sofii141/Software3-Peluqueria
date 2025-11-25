using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Interfaces
{
    public interface IServicioRepository
    {
        Task<IEnumerable<Servicio>> GetAllAsync();
        Task<Servicio?> GetByIdAsync(int id);
        Task<IEnumerable<Servicio>> GetByCategoriaIdAsync(int categoriaId);
        Task<Servicio> CreateAsync(Servicio servicio);
        Task<Servicio?> UpdateAsync(int id, Servicio servicio);
        Task<bool> InactivateAsync(int id);
        Task<bool> ExistsByNameAsync(string nombre);

        // Task<bool> HasFutureAppointmentsAsync(int id); // TODO: Agregar cuando conectes con Reservas
    }
}