using Peluqueria.Application.Dtos.Categoria;

namespace Peluqueria.Application.Dtos.Servicio
{
    /// <summary>
    /// Representación completa de un servicio para el cliente (Frontend).
    /// </summary>
    public class ServicioDto
    {
        /// <summary>
        /// Identificador único del servicio.
        /// </summary>
        /// <example>5</example>
        public int Id { get; set; }

        /// <summary>
        /// Nombre comercial del servicio.
        /// </summary>
        /// <example>Corte Caballero Clásico</example>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción comercial.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Duración en minutos. Dato clave para calcular horarios.
        /// </summary>
        /// <example>45</example>
        public int DuracionMinutos { get; set; }

        /// <summary>
        /// Precio numérico del servicio.
        /// </summary>
        /// <example>35000.00</example>
        public double Precio { get; set; }

        /// <summary>
        /// URL completa de la imagen del servicio.
        /// </summary>
        /// <example>https://api.dominio.com/images/corte1.jpg</example>
        public string Imagen { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en la que se registró el servicio en el sistema.
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Indica si el servicio está activo y reservable.
        /// </summary>
        public bool Disponible { get; set; }

        /// <summary>
        /// Objeto con la información de la categoría a la que pertenece.
        /// </summary>
        public CategoriaDto Categoria { get; set; } = null!;
    }
}