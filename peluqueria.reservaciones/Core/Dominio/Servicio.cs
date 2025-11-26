
namespace peluqueria.reservaciones.Core.Dominio
{
    public class Servicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public int CategoriaId { get; set; } 
        public bool Disponible { get; set; }
    }
}