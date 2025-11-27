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

        // Búsquedas
        Task<List<Reservacion>> BuscarReservasPorClienteAsync(string clienteIdentificacion);

        Task<List<Reservacion>> BuscarReservasPorEstilistaAsync(int estilistaId, DateOnly fecha);

        // Busca si ya existen reservas que choquen con el horario propuesto
        Task<List<Reservacion>> ObtenerReservasConflictivasAsync(int estilistaId, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin);
    }
}