using System.Text.Json.Serialization;

namespace Peluqueria.API.Errors
{
    public class ErrorResponse
    {
        [JsonPropertyName("codigoError")]
        public string CodigoError { get; set; } = string.Empty;

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [JsonPropertyName("codigoHttp")]
        public int CodigoHttp { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("metodo")]
        public string Metodo { get; set; } = string.Empty;
    }
}