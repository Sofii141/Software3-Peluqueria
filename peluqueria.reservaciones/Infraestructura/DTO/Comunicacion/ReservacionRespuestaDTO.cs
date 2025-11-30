using System;

/*
 @author: Juan Dabid Moran
    @description: DTO para la respuesta de las solicitudes de una reservación.
 */

namespace peluqueria.reservaciones.Infraestructura.DTO.Comunicacion
{
    public class ReservacionRespuestaDTO
    {
        public int ReservacionId { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }

        public string EstilistaNombre { get; set; } = string.Empty;
        public string ServicioNombre { get; set; } = string.Empty;
        public decimal ServicioPrecio { get; set; }
        public int TiempoAtencion { get; set; }

        public string Estado { get; set; } = string.Empty;
        public string NombreIdentificacion { get; set; } = string.Empty;
    }
}