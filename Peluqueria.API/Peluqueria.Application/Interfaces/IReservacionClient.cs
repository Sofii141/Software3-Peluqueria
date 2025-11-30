using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IReservacionClient
    {
        Task<bool> TieneReservasEstilista(int estilistaId);
        Task<bool> TieneReservasServicio(int servicioId);
        Task<bool> TieneReservasCategoria(int categoriaId);

        // Aquí usamos System.DayOfWeek explícitamente
        Task<bool> TieneReservasEnDia(int estilistaId, System.DayOfWeek dia);

        // Aquí usamos System.DateOnly explícitamente
        Task<bool> TieneReservasEnRango(int estilistaId, System.DateOnly inicio, System.DateOnly fin);

        // ESTA ES LA LÍNEA DEL PROBLEMA (BLINDADA)
        Task<bool> TieneReservasEnDescanso(int estilistaId, System.DayOfWeek dia, System.TimeSpan inicio, System.TimeSpan fin);
    }
}