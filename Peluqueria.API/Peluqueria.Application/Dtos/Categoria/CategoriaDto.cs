namespace Peluqueria.Application.Dtos.Categoria
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EstaActiva { get; set; }
    }
}