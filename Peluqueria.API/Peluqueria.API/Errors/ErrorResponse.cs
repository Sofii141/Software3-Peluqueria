using System.Text.Json.Serialization;

namespace Peluqueria.API.Errors
{
    /// <summary>
    /// Representa la estructura estandarizada de respuesta que la API devuelve al cliente
    /// cuando ocurre una excepción o un error controlado durante el procesamiento de una solicitud.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Obtiene o establece el código de error interno de negocio (ej. 'G-ERROR-001') 
        /// para facilitar la identificación del tipo de problema en el frontend.
        /// </summary>
        [JsonPropertyName("codigoError")]
        public string CodigoError { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el mensaje descriptivo del error, legible para el usuario o desarrollador cliente.
        /// </summary>
        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el código de estado HTTP asociado al error (ej. 400, 404, 500).
        /// </summary>
        [JsonPropertyName("codigoHttp")]
        public int CodigoHttp { get; set; }

        /// <summary>
        /// Obtiene o establece la ruta relativa (URL) de la solicitud que generó el error.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el verbo HTTP (GET, POST, PUT, DELETE) utilizado en la solicitud fallida.
        /// </summary>
        [JsonPropertyName("metodo")]
        public string Metodo { get; set; } = string.Empty;
    }
}