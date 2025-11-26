using System.Text.Json.Serialization;

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