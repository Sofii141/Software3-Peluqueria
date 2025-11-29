namespace Peluqueria.Application.Interfaces
{
    public interface IReservacionClient
    {
        Task<bool> TieneReservasEstilista(int estilistaId);
        Task<bool> TieneReservasServicio(int servicioId);
        Task<bool> TieneReservasCategoria(int categoriaId);
        Task<bool> TieneReservasEnDia(int estilistaId, DayOfWeek dia);
        Task<bool> TieneReservasEnRango(int estilistaId, DateOnly inicio, DateOnly fin);
        Task<bool> TieneReservasEnDescanso(int estilistaId, DayOfWeek dia, TimeSpan inicio, TimeSpan fin);
    }
}
