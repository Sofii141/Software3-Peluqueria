using System.Text.Json.Serialization;

/*
 @Autor: Juan David Moran
 @descripcion: DTO para la informacion de los dias laborales en los horarios de los estilista en los eventos de mensajeria
 */

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
{
    public class DiaHorarioEventoDTO
    {
        // DayOfWeek se serializa como int (0=Domingo, 1=Lunes, etc.)
        [JsonPropertyName("DiaSemana")]
        public DayOfWeek DiaSemana { get; set; }

        [JsonPropertyName("HoraInicio")]
        public TimeSpan HoraInicio { get; set; }

        [JsonPropertyName("HoraFin")]
        public TimeSpan HoraFin { get; set; }

        [JsonPropertyName("EsLaborable")]
        public bool EsLaborable { get; set; }
    }
}