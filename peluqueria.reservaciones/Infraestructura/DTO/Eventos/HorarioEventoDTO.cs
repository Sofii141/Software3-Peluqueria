// Archivo: Infraestructura/DTO/Eventos/HorarioBaseEstilistaEventDto.cs
using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
{
    public class HorarioEventoDTO
    {
        [JsonPropertyName("EstilistaId")]
        public int EstilistaId { get; set; }

        [JsonPropertyName("HorariosSemanales")]
        public List<DiaHorarioEventoDTO> HorariosSemanales { get; set; } = new();
    }
}