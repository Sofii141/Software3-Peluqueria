using System;
using System.ComponentModel.DataAnnotations;

namespace peluqueria.reservaciones.Aplicacion.Comandos
{
    public class CrearReservacionComando
    {
        [Required] public DateOnly Fecha { get; set; }
        [Required] public TimeOnly HoraInicio { get; set; }
        [Required] public int EstilistaId { get; set; }
        [Required] public int ServicioId { get; set; }
        [Required] public string ClienteIdentificacion { get; set; } = string.Empty;
    }
}