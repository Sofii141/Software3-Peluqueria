namespace Peluqueria.Application.Dtos.Events
{
    public class HorarioBaseEstilistaEventDto
    {
        public int EstilistaId { get; set; }
        public List<DiaHorarioEventDto> HorariosSemanales { get; set; } = new List<DiaHorarioEventDto>();
    }
}
