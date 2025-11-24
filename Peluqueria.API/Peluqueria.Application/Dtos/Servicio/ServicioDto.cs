using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Dtos;

namespace Peluqueria.Application.Dtos.Servicio
{
    public class ServicioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; 
        public string Descripcion { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public double Precio { get; set; }
        public string Imagen { get; set; } = string.Empty; 
        public DateTime FechaCreacion { get; set; }
        public bool Disponible { get; set; }
        public CategoriaDto Categoria { get; set; } = null!; 
    }
}