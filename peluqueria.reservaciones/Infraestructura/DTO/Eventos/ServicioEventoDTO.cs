// Archivo: Infraestructura/DTO/Eventos/ServicioEventIncomingDto.cs

using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
{
    public class ServicioEventoDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("duracionMinutos")]
        public int DuracionMinutos { get; set; }

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; } // El deserializador convertirá el número del JSON a decimal

        [JsonPropertyName("categoriaId")]
        public int CategoriaId { get; set; }

        [JsonPropertyName("disponible")]
        public bool Disponible { get; set; }

        [JsonPropertyName("accion")]
        public string Accion { get; set; } = string.Empty; // "CREADO", "ACTUALIZADO", "INACTIVADO"
    }
}