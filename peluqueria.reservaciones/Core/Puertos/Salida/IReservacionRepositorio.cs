using peluqueria.reservaciones.Core.Dominio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
    public interface IReservacionRepositorio
    {
        Task<Reservacion> GuardarAsync(Reservacion reservacion);
        Task<Reservacion?> ObtenerPorIdAsync(int id);
        Task ActualizarAsync(Reservacion reservacion); 
        Task CancelarAsync(int id);

        Task<List<Reservacion>> BuscarReservasPorClienteAsync(string clienteIdentificacion);

        Task<List<Reservacion>> BuscarReservasPorEstilistaAsync(int estilistaId, DateOnly fecha);

        Task CambiarEstadoAsync(int reservacionId, string nuevoEstado);

        Task<List<Reservacion>> BuscarReservasEstilistaRangoAsync(int estilistaId, DateOnly fechaInicio, DateOnly fechaFin);

        Task<List<Reservacion>> ObtenerReservasConflictivasAsync(int estilistaId, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin);

        Task<bool> TieneReservasFuturasEstilistaAsync(int estilistaId);
        Task<bool> TieneReservasFuturasServicioAsync(int servicioId);
        Task<bool> TieneReservasFuturasCategoriaAsync(int categoriaId);
        Task<bool> TieneConflictoHorarioAsync(int estilistaId, DayOfWeek diaSemana);
        Task<bool> TieneConflictoRangoAsync(int estilistaId, DateOnly inicio, DateOnly fin);
        Task<bool> TieneConflictoDescansoAsync(int estilistaId, DayOfWeek dia, TimeOnly inicio, TimeOnly fin);
    }
}