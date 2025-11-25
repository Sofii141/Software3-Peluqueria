using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Servicio
{
    public class CreateServicioRequestDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 5 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración es obligatoria.")]
        [Range(45, 480, ErrorMessage = "La duración debe ser entre 45 y 480 minutos (8 horas).")]
        public int DuracionMinutos { get; set; }
        public string Precio { get; set; } = string.Empty;
        public bool Disponible { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int CategoriaId { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}