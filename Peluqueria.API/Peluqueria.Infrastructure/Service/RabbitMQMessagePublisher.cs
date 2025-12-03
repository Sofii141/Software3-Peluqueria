using Peluqueria.Application.Interfaces;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Threading;

namespace Peluqueria.Infrastructure.Service
{
    /// <summary>
    /// Servicio encargado de publicar mensajes en el bus de eventos (RabbitMQ).
    /// </summary>
    /// <remarks>
    /// Implementa <see cref="IDisposable"/> para asegurar que la conexión TCP con el broker se cierre correctamente
    /// cuando la aplicación se detiene.
    /// Configura los Exchanges como tipo "Topic" para permitir enrutamiento flexible mediante Routing Keys.
    /// </remarks>
    public class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
    {
        private IConnection? _connection;
        private readonly ILogger<RabbitMQMessagePublisher> _logger;
        private bool _disposed = false;

        public RabbitMQMessagePublisher(IConfiguration config, ILogger<RabbitMQMessagePublisher> logger)
        {
            _logger = logger;

            string hostName = config["RabbitMQ:HostName"] ?? "localhost";
            string portString = config["RabbitMQ:Port"] ?? "5672";
            string userName = config["RabbitMQ:UserName"] ?? "guest";
            string password = config["RabbitMQ:Password"] ?? "guest";

            if (!int.TryParse(portString, NumberStyles.None, CultureInfo.InvariantCulture, out int port))
            {
                _logger.LogError("El puerto de RabbitMQ '{Port}' no es un número válido. Usando 5672.", portString);
                port = 5672;
            }

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
            };

            try
            {
                // Intentamos conectar al inicio. Si falla, la app inicia pero sin mensajería.
                _connection = factory.CreateConnection();
                _logger.LogInformation("Conectado exitosamente a RabbitMQ en {HostName}:{Port}", hostName, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CRÍTICO al conectar con RabbitMQ. Verificar Docker y configuración.");
                _connection = null;
            }
        }

        /// <summary>
        /// Publica un mensaje genérico en RabbitMQ.
        /// </summary>
        /// <typeparam name="T">Tipo del DTO a enviar.</typeparam>
        /// <param name="message">El objeto de datos.</param>
        /// <param name="routingKey">La llave de enrutamiento (ej: "estilista.creado").</param>
        /// <param name="exchangeName">El nombre del Exchange destino.</param>
        public Task PublishAsync<T>(T message, string routingKey, string exchangeName) where T : class
        {
            // Verificación de seguridad: si no hay conexión, no hacemos nada para evitar crashes.
            if (_disposed || _connection == null || !_connection.IsOpen)
            {
                _logger.LogError("Intento de publicar en RabbitMQ con conexión nula, cerrada o desechada. Mensaje no enviado: {Key}", routingKey);
                return Task.CompletedTask;
            }

            // Serialización a JSON UTF-8 (Estándar para microservicios)
            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            // Creamos un canal temporal para este mensaje
            using var channel = _connection.CreateModel();

            // 1. Declaración del Exchange (Idempotente: si existe no hace nada)
            channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true // Importante: Sobrevive a reinicios del broker
            );

            // 2. Propiedades del mensaje
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; // El mensaje se guarda en disco en RabbitMQ

            // 3. Publicación
            channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );

            _logger.LogDebug("Mensaje publicado con éxito: Exchange={ExchangeName}, Key={RoutingKey}", exchangeName, routingKey);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Limpieza de recursos no administrados (Conexión TCP).
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("Cerrando la conexión de RabbitMQ.");

            if (_connection != null)
            {
                if (_connection.IsOpen)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}