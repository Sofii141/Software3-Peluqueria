using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
{
    public class DescansoEventoDTO
    {
        [JsonPropertyName("EstilistaId")]
        public int EstilistaId { get; set; }

        [JsonPropertyName("DescansosFijos")]
        public List<DiaHorarioEventoDTO> DescansosFijos { get; set; } = new();
    }
}