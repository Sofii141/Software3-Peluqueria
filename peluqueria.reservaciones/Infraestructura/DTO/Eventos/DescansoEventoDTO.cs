using System.Text.Json.Serialization;

/*
 @Autor: Juan David Moran
 @descripcion: DTO para la informacion de los descansos fijos de un estilista en los eventos de mensajeria
 */

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