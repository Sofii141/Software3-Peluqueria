// peluqueria.reservaciones.Aplicacion.Mapeo/ReservacionMapper.cs

using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Infraestructura.DTO.Comunicacion;

namespace peluqueria.reservaciones.Aplicacion.Mapeo
{
    public static class ReservacionMapper
    {
        public static ReservacionRespuestaDTO ToRespuestaDTO(Reservacion reservacion)
        {
            return new ReservacionRespuestaDTO
            {
                ReservacionId = reservacion.Id,
                Fecha = reservacion.Fecha,
                HoraInicio = reservacion.HoraInicio,
                HoraFin = reservacion.HoraFin,

                // Propiedades enriquecidas para la respuesta del cliente
                EstilistaNombre = reservacion.Estilista?.NombreCompleto ?? "N/A",
                ServicioNombre = reservacion.Servicio?.Nombre ?? "N/A",
                ServicioPrecio = reservacion.Servicio?.Precio ?? 0,
                TiempoAtencion = reservacion.TiempoAtencion,
                Estado = reservacion.Estado,
                NombreIdentificacion = reservacion.ClienteIdentificacion ?? "N/A"

            };
        }
    }
}