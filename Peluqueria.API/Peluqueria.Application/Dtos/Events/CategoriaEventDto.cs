namespace Peluqueria.Application.Dtos.Events
{
    public class CategoriaEventDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EstaActiva { get; set; } // Estado de baja lógica
        public string Accion { get; set; } = string.Empty; // CREADA, ACTUALIZADA, INACTIVADA
    }
}
