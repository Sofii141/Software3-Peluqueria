namespace peluqueria.reservaciones.Core.Dominio
{
    public class HorarioBase
    {
        public int EstilistaId { get; set; }
        public List<DiaHorario> HorariosSemanales { get; set; } = new List<DiaHorario>();
    }
}