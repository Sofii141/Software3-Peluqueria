using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Servicio
{
    public class UpdateServicioRequestDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public string Precio { get; set; } = string.Empty;
        public bool Disponible { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}
