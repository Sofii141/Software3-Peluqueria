using peluqueria.reservaciones.Aplicacion.Comandos;
using peluqueria.reservaciones.Infraestructura.DTO.Comunicacion;
using System.Collections.Generic;
using System.Threading.Tasks;

/*
 @autor: Juan David Moran
    @descripcion: Interfaz que define las operaciones de manejo para las reservaciones crear, reprogramar, cancelar y busquedas 
    de reservaciones para clientes y estilistas.
 */

namespace peluqueria.reservaciones.Core.Puertos.Entrada
{
    public interface IReservacionManejador
    {

        Task<ReservacionRespuestaDTO> CrearReservacionAsync(CrearReservacionComando comando);

        Task<ReservacionRespuestaDTO> ReprogramarReservacionAsync(int reservacionId, ReservacionPeticionDTO peticion);

        Task CancelarReservacionAsync(int reservacionId);

        Task<List<ReservacionRespuestaDTO>> ConsultarReservacionesClienteAsync(string clienteIdentificacion);

        Task CambiarEstadoReservacionAsync(CambioEstadoDTO peticion);

        Task<List<ReservacionRespuestaDTO>> ConsultarReservasEstilistaRangoAsync(PeticionReservasEstilistaDTO peticion);

        Task<List<ReservacionRespuestaDTO>> ConsultarReservasEstilistaFechaAsync(PeticionReservaEstilistaFechaDTO peticion);

        Task<List<ReservacionRespuestaDTO>> ConsultarTodasLasReservacionesAsync();
    }
}