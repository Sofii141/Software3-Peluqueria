using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Categoria
{
    public class UpdateCategoriaRequestDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public bool EstaActiva { get; set; }
    }
}
