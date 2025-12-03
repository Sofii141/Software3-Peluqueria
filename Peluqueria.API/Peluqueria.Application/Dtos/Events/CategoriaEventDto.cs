namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Evento de sincronización para cambios en las Categorías.
    /// </summary>
    /// <remarks>
    /// Se publica en RabbitMQ (Exchange: `categoria_exchange`) cuando una categoría es creada, modificada o inactivada en el Monolito.
    /// </remarks>
    public class CategoriaEventDto
    {
        /// <summary>
        /// ID único de la categoría en la base de datos maestra.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre descriptivo de la categoría.
        /// </summary>
        /// <example>Cortes de Cabello</example>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la categoría está disponible para asociar nuevos servicios.
        /// </summary>
        public bool EstaActiva { get; set; }

        /// <summary>
        /// Tipo de operación realizada que disparó el evento.
        /// Valores posibles: "CREADA", "ACTUALIZADA", "INACTIVADA".
        /// </summary>
        /// <example>CREADA</example>
        public string Accion { get; set; } = string.Empty;
    }
}