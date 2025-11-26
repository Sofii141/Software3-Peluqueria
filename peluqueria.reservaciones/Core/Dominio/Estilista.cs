namespace peluqueria.reservaciones.Core.Dominio
{
    public class Estilista
    {
        public int Id { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
    }
}