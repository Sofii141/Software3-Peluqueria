using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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