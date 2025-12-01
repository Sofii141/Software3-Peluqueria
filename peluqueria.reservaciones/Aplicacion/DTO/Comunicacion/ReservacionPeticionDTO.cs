using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/*
 @author: Juan Dabid Moran
	@description: DTO para la recepción de datos para la creación de una nueva reservación.
 */

namespace peluqueria.reservaciones.Aplicacion.DTO.Comunicacion
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