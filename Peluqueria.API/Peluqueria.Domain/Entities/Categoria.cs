namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Representa una agrupación de servicios (ej. "Cortes", "Manicura").
    /// </summary>
    /// <remarks>
    /// Tabla: <c>Categorias</c>.
    /// Se utiliza para organizar el catálogo en el Frontend y aplicar filtros.
    /// </remarks>
    public class Categoria
    {
        /// <summary>
        /// Clave primaria (PK).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre descriptivo de la categoría. Debe ser único en el sistema.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Indica el estado del registro.
        /// <br/>
        /// <c>true</c>: Visible. <c>false</c>: Borrado lógico (Soft Delete).
        /// </summary>
        public bool EstaActiva { get; set; } = true;

        /// <summary>
        /// Propiedad de navegación (1:N). Lista de servicios que pertenecen a esta categoría.
        /// </summary>
        public List<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}