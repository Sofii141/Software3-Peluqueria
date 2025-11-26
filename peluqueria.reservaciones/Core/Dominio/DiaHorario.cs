namespace peluqueria.reservaciones.Core.Dominio
{
    public class DiaHorario
    {
        public DayOfWeek DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool EsLaborable { get; set; }
    }
}