
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Application.Dtos.Servicio
{
    public class CreateServicioRequestDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Precio { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        [Required]
        public int CategoriaId { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}
