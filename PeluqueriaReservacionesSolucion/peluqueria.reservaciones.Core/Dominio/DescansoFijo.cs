namespace peluqueria.reservaciones.Core.Dominio
{
	public class DescansoFijo
	{
		public int EstilistaId { get; set; }
		public List<DiaHorario> DescansosFijos { get; set; } = new List<DiaHorario>();
	}
}