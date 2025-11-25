using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Categoria
{
    public class UpdateCategoriaRequestDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        public bool EstaActiva { get; set; }
    }
}