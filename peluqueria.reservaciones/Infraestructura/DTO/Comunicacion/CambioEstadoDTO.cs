using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/*
 @author: Juan Dabid Moran
    @description: DTO para la recepción de datos para el cambio de estado de una reservación.
 */

namespace peluqueria.reservaciones.Infraestructura.DTO.Comunicacion
{
    public class CambioEstadoDTO
    {
        [JsonPropertyName("reservacionId")]
        public int ReservacionId { get; set; }

        [JsonPropertyName("nuevoEstado")]
        public string NuevoEstado { get; set; } = string.Empty;
    }
}