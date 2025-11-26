namespace peluqueria.reservaciones.Core.Dominio
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EstaActiva { get; set; }
    }
}