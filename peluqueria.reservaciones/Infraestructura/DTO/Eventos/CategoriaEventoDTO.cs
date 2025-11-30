using System.Text.Json.Serialization;

/*
 @autor: Juan David Moran
 @descripcion: DTO para la categoria de servicio en los eventos de mensajeria.
 */

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
{
    public class CategoriaEventoDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("estaActiva")]
        public bool EstaActiva { get; set; }

        [JsonPropertyName("accion")]
        public string Accion { get; set; } = string.Empty; // "CREADA", "ACTUALIZADA", "INACTIVADA"
    }
}