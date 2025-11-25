namespace Peluqueria.Application.Dtos.Estilista
{
    public class BloqueoResponseDto
    {
        public int Id { get; set; } 
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Razon { get; set; } = string.Empty;
    }
}