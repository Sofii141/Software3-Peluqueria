namespace Peluqueria.Application.Dtos.Events
{
    public class ServicioEventDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public double Precio { get; set; } // <--- AÑADIDO
        public int CategoriaId { get; set; } // <--- AÑADIDO
        public bool Disponible { get; set; }
        public string Accion { get; set; } = string.Empty; // CREADO, ACTUALIZADO, 
    }
}
