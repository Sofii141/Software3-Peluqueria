namespace Peluqueria.Domain.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public List<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}