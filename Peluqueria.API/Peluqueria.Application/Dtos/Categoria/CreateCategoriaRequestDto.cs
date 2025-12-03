using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Categoria
{
    /// <summary>
    /// DTO que encapsula los datos requeridos para la creación de una nueva categoría.
    /// </summary>
    public class CreateCategoriaRequestDto
    {
        /// <summary>
        /// Nombre de la categoría a crear. Debe tener una longitud entre 3 y 50 caracteres.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
    }
}