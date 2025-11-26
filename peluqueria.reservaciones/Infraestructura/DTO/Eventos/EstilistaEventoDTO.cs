using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
{
    public class EstilistaEventoDTO
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("NombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("IdentityId")]
        public string IdentityId { get; set; } = string.Empty;

        [JsonPropertyName("EstaActivo")]
        public bool EstaActivo { get; set; }

        [JsonPropertyName("Accion")]
        public string Accion { get; set; } = string.Empty; // "CREADO", "ACTUALIZADO", "INACTIVADO"

    }
}