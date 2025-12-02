namespace peluqueria.reservaciones.Core.Dominio
{
    public class Reservacion
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string ClienteIdentificacion { get; set; }
        public int ServicioId { get; set; }
        public int EstilistaId { get; set; }
        public Cliente Cliente { get; set; } = new Cliente();
        public Servicio Servicio { get; set; } = new Servicio();
        public Estilista Estilista { get; set; } = new Estilista();
        public int TiempoAtencion { get; set; }
    }
}