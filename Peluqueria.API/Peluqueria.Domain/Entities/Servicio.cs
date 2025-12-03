using System.ComponentModel.DataAnnotations.Schema;

namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Representa un producto o servicio ofrecido por la peluquería.
    /// </summary>
    /// <remarks>
    /// Tabla: <c>Servicios</c>.
    /// Es la entidad central para la facturación y la agenda.
    /// </remarks>
    public class Servicio
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Tiempo estimado de ejecución en minutos.
        /// </summary>
        /// <remarks>
        /// Este valor es CRÍTICO para el microservicio de reservas, ya que define el tamaño del "slot" en la agenda.
        /// </remarks>
        public int DuracionMinutos { get; set; } = 45;

        /// <summary>
        /// Costo del servicio. Mapeado como <c>decimal(18, 2)</c> en SQL Server para precisión monetaria.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public double Precio { get; set; }

        /// <summary>
        /// Nombre del archivo de imagen almacenado en el servidor.
        /// </summary>
        public string Imagen { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Disponibilidad para nuevas reservas. Si es false, no aparece en el catálogo público.
        /// </summary>
        public bool Disponible { get; set; }

        // --- RELACIONES ---

        /// <summary>
        /// Clave foránea (FK) hacia la Categoría.
        /// </summary>
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;

        /// <summary>
        /// Relación Muchos a Muchos (M:N) con Estilistas.
        /// Define qué profesionales saben realizar este servicio.
        /// </summary>
        public ICollection<EstilistaServicio> EstilistasAsociados { get; set; } = new List<EstilistaServicio>();
    }
}