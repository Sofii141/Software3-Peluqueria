using System.Text.Json.Serialization;

/*
 @Autor: Juan David Moran
 @descripcion: DTO para la informacion de los rangos de dias libres de los estilistas en los eventos de mensajeria
 */

namespace peluqueria.reservaciones.Infraestructura.Eventos
{
    public class RangoLibreEventoDTO
    {
        [JsonPropertyName("EstilistaId")]
        public int EstilistaId { get; set; }

        [JsonPropertyName("FechaInicioBloqueo")]
        public DateTime FechaInicioBloqueo { get; set; }

        [JsonPropertyName("FechaFinBloqueo")]
        public DateTime FechaFinBloqueo { get; set; }

        [JsonPropertyName("Accion")]
        public string Accion { get; set; } = string.Empty; // "CREADO", "ACTUALIZADO", "ELIMINADO"
    }
}