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

namespace Peluqueria.Infrastructure.Service // Asegúrate de que el namespace sea correcto
{
    public class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
    {
        // Se inicializa a null para satisfacer el constructor que puede fallar.
        private IConnection? _connection;
        private readonly ILogger<RabbitMQMessagePublisher> _logger;
        private bool _disposed = false;

        public RabbitMQMessagePublisher(IConfiguration config, ILogger<RabbitMQMessagePublisher> logger)
        {
            _logger = logger;

            // Obtener configuración de appsettings.json
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
                // Crear la conexión
                _connection = factory.CreateConnection();
                _logger.LogInformation("Conectado exitosamente a RabbitMQ en {HostName}:{Port}", hostName, port);
            }
            catch (Exception ex)
            {
                // Manejo de error si RabbitMQ no está disponible
                _logger.LogError(ex, "Error CRÍTICO al conectar con RabbitMQ. Verificar Docker y configuración.");
                _connection = null;
            }
        }

        public Task PublishAsync<T>(T message, string routingKey, string exchangeName) where T : class
        {
            // Verificación de conexión antes de publicar
            if (_disposed || _connection == null || !_connection.IsOpen)
            {
                _logger.LogError("Intento de publicar en RabbitMQ con conexión nula, cerrada o desechada. Mensaje no enviado: {Key}", routingKey);
                return Task.CompletedTask;
            }

            // Serializa el objeto a bytes JSON (UTF-8)
            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            // Se usa 'using' para el canal para asegurar que se deseche correctamente
            using var channel = _connection.CreateModel();

            // 1. Declaración del Exchange (Tipo Topic para enrutamiento flexible)
            channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true // La cola y el exchange sobreviven a reinicios
            );

            // 2. Propiedades del mensaje (Persistente)
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

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

        // Implementación de IDisposable para una limpieza correcta
        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("Cerrando la conexión de RabbitMQ.");

            if (_connection != null)
            {
                if (_connection.IsOpen)
                {
                    // Cerrar y luego Desechar
                    _connection.Close();
                }
                _connection.Dispose();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}