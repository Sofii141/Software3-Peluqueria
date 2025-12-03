namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Representación mínima de un servicio asociado a un estilista.
    /// </summary>
    public class EstilistaServicioMinimalEventDto
    {
        /// <summary>
        /// ID del servicio.
        /// </summary>
        public int ServicioId { get; set; }

        /// <summary>
        /// Duración específica del servicio (se envía por redundancia/optimización).
        /// </summary>
        public int DuracionMinutos { get; set; }
    }
}