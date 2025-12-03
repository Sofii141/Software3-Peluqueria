using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Servicio
{
    /// <summary>
    /// Objeto de transferencia para crear un nuevo servicio en el catálogo.
    /// </summary>
    /// <remarks>
    /// Se envía como `multipart/form-data` para soportar la carga de imagen.
    /// La duración es crítica para el cálculo de disponibilidad en la agenda.
    /// </remarks>
    public class CreateServicioRequestDto
    {
        /// <summary>
        /// Nombre comercial del servicio. Debe ser único.
        /// </summary>
        /// <example>Corte Fade con Barba</example>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 5 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada de lo que incluye el servicio para el cliente.
        /// </summary>
        /// <example>Incluye lavado, corte con máquina y tijera, perfilado de barba y toalla caliente.</example>
        [StringLength(500, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Tiempo estimado en minutos que toma realizar el servicio.
        /// </summary>
        /// <remarks>
        /// Regla de negocio: Mínimo 45 minutos, Máximo 480 minutos (8 horas).
        /// </remarks>
        /// <example>60</example>
        [Required(ErrorMessage = "La duración es obligatoria.")]
        [Range(45, 480, ErrorMessage = "La duración debe ser entre 45 y 480 minutos (8 horas).")]
        public int DuracionMinutos { get; set; }

        /// <summary>
        /// Precio del servicio como cadena de texto (se permite formato simple).
        /// </summary>
        /// <remarks>
        /// El backend intentará parsear este valor. Use punto o coma para decimales si es necesario.
        /// </remarks>
        /// <example>35000</example>
        public string Precio { get; set; } = string.Empty;

        /// <summary>
        /// Define si el servicio aparece visible para ser reservado inmediatamente.
        /// </summary>
        /// <example>true</example>
        public bool Disponible { get; set; }

        /// <summary>
        /// ID de la categoría a la que pertenece el servicio.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int CategoriaId { get; set; }

        /// <summary>
        /// (Opcional) Archivo de imagen promocional del servicio.
        /// </summary>
        public IFormFile? Imagen { get; set; }
    }
}