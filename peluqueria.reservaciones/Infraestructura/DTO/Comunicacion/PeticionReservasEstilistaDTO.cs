using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/*
 @author: Juan Dabid Moran
    @description: DTO para la petición de consular todas  las reservaciones de un estilista en un rango de fechas.
 */

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