namespace Peluqueria.Application.Dtos.Categoria
{
    /// <summary>
    /// Objeto de transferencia de datos que representa una categoría de servicios en el sistema.
    /// Utilizado principalmente para las respuestas de lectura (GET).
    /// </summary>
    public class CategoriaDto
    {
        /// <summary>
        /// Identificador único de la categoría.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre descriptivo de la categoría (ej. "Cortes", "Manicura").
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la categoría se encuentra activa en el sistema.
        /// </summary>
        public bool EstaActiva { get; set; }
    }
}