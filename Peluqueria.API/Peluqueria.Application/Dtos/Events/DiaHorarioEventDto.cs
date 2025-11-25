namespace Peluqueria.Application.Dtos.Events
{
    public class DiaHorarioEventDto
    {
        public DayOfWeek DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool EsLaborable { get; set; }
    }
}
