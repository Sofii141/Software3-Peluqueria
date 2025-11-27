using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace peluqueria.reservaciones.Infraestructura.DTO.Comunicacion
{
	public class ReservacionPeticionDTO
	{

		[Required]
		[JsonPropertyName("fecha")]
		public DateOnly Fecha { get; set; }

		[Required]
		[JsonPropertyName("horaInicio")]
		public TimeOnly HoraInicio { get; set; }

		[Required]
		[JsonPropertyName("estilistaId")]
		public int EstilistaId { get; set; }

		[Required]
		[JsonPropertyName("servicioId")]
		public int ServicioId { get; set; }

		[Required]
		[JsonPropertyName("clienteIdentificacion")]
		public string ClienteIdentificacion { get; set; } = string.Empty;
	}
}