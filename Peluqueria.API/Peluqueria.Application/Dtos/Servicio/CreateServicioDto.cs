using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Servicio
{
    public class CreateServicioRequestDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Range(45, int.MaxValue, ErrorMessage = "La duración mínima es de 45 minutos.")] // VALIDACIÓN HU
        public int DuracionMinutos { get; set; } // <<-- ATRIBUTO FALTANTE AGREGADO

        public string Precio { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int CategoriaId { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}