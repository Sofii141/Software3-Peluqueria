namespace Peluqueria.Application.Dtos.Estilista
{
    /// <summary>
    /// Respuesta que representa un bloqueo existente (Vacaciones) de un estilista.
    /// </summary>
    public class BloqueoResponseDto
    {
        /// <summary>
        /// ID interno del bloqueo. Útil para eliminarlo o editarlo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha de inicio del periodo no disponible.
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin del periodo no disponible.
        /// </summary>
        public DateTime FechaFin { get; set; }

        /// <summary>
        /// Motivo del bloqueo.
        /// </summary>
        public string Razon { get; set; } = string.Empty;
    }
}