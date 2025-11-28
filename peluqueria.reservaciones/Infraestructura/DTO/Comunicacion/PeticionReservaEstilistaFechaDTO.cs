using System;
using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Comunicacion
{
    public class PeticionReservaEstilistaFechaDTO
    {
        [JsonPropertyName("estilistaId")]
        public int EstilistaId { get; set; }

        [JsonPropertyName("fecha")]
        public DateOnly Fecha { get; set; }
    }
}