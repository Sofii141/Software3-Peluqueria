using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Comunicacion
{
    public class PeticionReservasEstilistaDTO
    {
        [JsonPropertyName("estilistaId")]
        public int EstilistaId { get; set; }

        [JsonPropertyName("fechaInicio")]
        public DateOnly FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateOnly FechaFin { get; set; }  
    }
}