namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Servicio de orquestación para la sincronización inicial de datos.
    /// </summary>
    public interface IDataSyncService
    {
        /// <summary>
        /// Envía todos los datos maestros (Servicios, Categorías, Estilistas, Horarios) a RabbitMQ.
        /// </summary>
        /// <remarks>
        /// Se ejecuta al iniciar la aplicación para asegurar que el Microservicio tenga datos actualizados.
        /// </remarks>
        Task SincronizarTodoAsync();
    }
}