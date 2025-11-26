using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Eventos
{
    public class ClienteEventoDTO
    {
        [JsonPropertyName("IdentityId")]
        public string IdentityId { get; set; } = string.Empty; 

        [JsonPropertyName("Username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("NombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("Email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("Telefono")]
        public string Telefono { get; set; } = string.Empty;
    }
}