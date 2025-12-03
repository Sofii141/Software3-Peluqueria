using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Categoria
{
    /// <summary>
    /// DTO utilizado para la actualización de los datos de una categoría existente.
    /// </summary>
    public class UpdateCategoriaRequestDto
    {
        /// <summary>
        /// Nuevo nombre para la categoría. Debe mantener las reglas de longitud establecidas.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Define el estado de actividad de la categoría.
        /// Permite reactivar o dar de baja lógica a la categoría.
        /// </summary>
        public bool EstaActiva { get; set; }
    }
}