using System;
using System.Text.Json.Serialization;

/*
 @author: Juan Dabid Moran
    @description: DTO para la petición de visualizar las reservaciones de un estilista en una fecha específica.
 */

namespace peluqueria.reservaciones.Aplicacion.DTO.Comunicacion
{
    public class PeticionReservaEstilistaFechaDTO
    {
        [JsonPropertyName("estilistaId")]
        public int EstilistaId { get; set; }

        [JsonPropertyName("fecha")]
        public DateOnly Fecha { get; set; }
    }
}