using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
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