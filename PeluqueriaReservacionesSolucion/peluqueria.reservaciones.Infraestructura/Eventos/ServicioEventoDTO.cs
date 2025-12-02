using System.Text.Json.Serialization;

/*
 @Autor: Juan David Moran
 @descripcion: DTO para la informacion de los servicios en los eventos de mensajeria
 */

namespace peluqueria.reservaciones.Infraestructura.Eventos
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
        public decimal Precio { get; set; } 

        [JsonPropertyName("categoriaId")]
        public int CategoriaId { get; set; }

        [JsonPropertyName("disponible")]
        public bool Disponible { get; set; }

        [JsonPropertyName("accion")]
        public string Accion { get; set; } = string.Empty;
    }
}