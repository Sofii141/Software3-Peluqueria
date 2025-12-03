namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Evento de sincronización para cambios en los Servicios.
    /// </summary>
    /// <remarks>
    /// Contiene la información esencial que el Microservicio de Reservas necesita para validar disponibilidad y calcular precios.
    /// Exchange: `servicio_exchange`.
    /// </remarks>
    public class ServicioEventDto
    {
        /// <summary>
        /// ID del servicio en el sistema maestro.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre comercial del servicio.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Duración estimada del servicio en minutos. Crítico para calcular la agenda.
        /// </summary>
        /// <example>60</example>
        public int DuracionMinutos { get; set; }

        /// <summary>
        /// Precio actual del servicio.
        /// </summary>
        public double Precio { get; set; }

        /// <summary>
        /// ID de la categoría a la que pertenece.
        /// </summary>
        public int CategoriaId { get; set; }

        /// <summary>
        /// Indica si el servicio se puede reservar actualmente.
        /// </summary>
        public bool Disponible { get; set; }

        /// <summary>
        /// Acción realizada: "CREADO", "ACTUALIZADO", "INACTIVADO".
        /// </summary>
        public string Accion { get; set; } = string.Empty;
    }
}