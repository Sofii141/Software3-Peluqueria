namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Abstracción para el bus de mensajería (RabbitMQ).
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publica un evento genérico en un Exchange específico.
        /// </summary>
        /// <typeparam name="T">Tipo del DTO del evento.</typeparam>
        /// <param name="message">El objeto del mensaje a enviar.</param>
        /// <param name="routingKey">Clave de enrutamiento (Topic) para filtrar el mensaje.</param>
        /// <param name="exchangeName">Nombre del Exchange en RabbitMQ.</param>
        Task PublishAsync<T>(T message, string routingKey, string exchangeName) where T : class;
    }
}