namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Tabla de configuración global del sistema (Singleton en BD).
    /// </summary>
    /// <remarks>
    /// Almacena variables que afectan la lógica de negocio general y no dependen de un usuario.
    /// </remarks>
    public class ParametroSistema
    {
        /// <summary>
        /// Usualmente siempre es 1, ya que solo hay una fila de configuración.
        /// </summary>
        public int Id { get; set; } = 1;

        /// <summary>
        /// Tiempo extra (en minutos) que se agrega al final de cada servicio para limpieza/preparación.
        /// </summary>
        /// <remarks>Regla: RN-BUFFER</remarks>
        public int BufferMinutos { get; set; } = 5;

        /// <summary>
        /// Tiempo máximo de retraso permitido al cliente antes de cancelar la cita.
        /// </summary>
        /// <remarks>Regla: RN-TOLERANCIA</remarks>
        public int ToleranciaLlegadaMinutos { get; set; } = 10;

        /// <summary>
        /// Duración mínima permitida para registrar un servicio en el sistema.
        /// </summary>
        /// <remarks>Regla: RN-DURACIÓN-MIN</remarks>
        public int DuracionMinimaServicioMinutos { get; set; } = 45;
    }
}