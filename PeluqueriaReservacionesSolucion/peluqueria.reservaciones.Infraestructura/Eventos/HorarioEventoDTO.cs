// Archivo: Infraestructura/DTO/Eventos/HorarioBaseEstilistaEventDto.cs
using System.Text.Json.Serialization;

/*
 @Autor: Juan David Moran
 @descripcion: DTO para la informacion de los horarios base de los estilistas en los eventos de mensajeria
 */

namespace peluqueria.reservaciones.Infraestructura.Eventos
{
    public class HorarioEventoDTO
    {
        [JsonPropertyName("EstilistaId")]
        public int EstilistaId { get; set; }

        [JsonPropertyName("HorariosSemanales")]
        public List<DiaHorarioEventoDTO> HorariosSemanales { get; set; } = new();
    }
}