using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Servicio
{
    /// <summary>
    /// Objeto para actualizar un servicio existente.
    /// </summary>
    /// <remarks>
    /// Nota importante: Cambiar la duración de un servicio NO afecta a las citas ya agendadas,
    /// pero sí modificará los bloques de tiempo para futuras reservas.
    /// </remarks>
    public class UpdateServicioRequestDto
    {
        /// <summary>
        /// Nuevo nombre del servicio.
        /// </summary>
        /// <example>Corte Fade Premium</example>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 5 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Nueva descripción del servicio.
        /// </summary>
        [StringLength(500, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Nueva duración en minutos.
        /// </summary>
        /// <example>75</example>
        [Required(ErrorMessage = "La duración es obligatoria.")]
        public int DuracionMinutos { get; set; }

        /// <summary>
        /// Nuevo precio.
        /// </summary>
        /// <example>40000</example>
        [Required(ErrorMessage = "El precio es obligatorio.")]
        public string Precio { get; set; } = string.Empty;

        /// <summary>
        /// Actualizar disponibilidad.
        /// </summary>
        public bool Disponible { get; set; }

        /// <summary>
        /// ID de la categoría (permite mover el servicio de categoría).
        /// </summary>
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int CategoriaId { get; set; }

        /// <summary>
        /// (Opcional) Nueva imagen. Si no se envía, se mantiene la anterior.
        /// </summary>
        public IFormFile? Imagen { get; set; }
    }
}