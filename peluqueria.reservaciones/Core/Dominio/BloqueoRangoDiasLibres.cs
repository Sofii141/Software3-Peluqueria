namespace peluqueria.reservaciones.Core.Dominio
{
    public class BloqueoRangoDiasLibres
    {
        public int EstilistaId { get; set; }
        public DateTime FechaInicioBloqueo { get; set; }
        public DateTime FechaFinBloqueo { get; set; }
        public string Accion { get; set; } = string.Empty;
    }
}