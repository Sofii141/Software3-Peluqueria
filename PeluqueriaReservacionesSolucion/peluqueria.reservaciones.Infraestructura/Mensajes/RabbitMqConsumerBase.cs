using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

/*
 @author: Juan David Moran
 @description: Clase base para consumidores de RabbitMQ en segundo plano.
 */

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    // Hereda de BackgroundService para correr en segundo plano siempre
    public abstract class RabbitMqConsumerBase : BackgroundService
    {
        private readonly IConnection _connection;
        protected readonly IModel _channel;
        protected string QueueName { get; set; } 

        public RabbitMqConsumerBase(IOptions<RabbitMqOptions> rabbitMqOptions, string queueName)
        {
            QueueName = queueName;

            var options = rabbitMqOptions.Value;

            var factory = new ConnectionFactory()
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                ProcessMessageAsync(message,routingKey).Wait();

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        protected abstract Task ProcessMessageAsync(string message,string routingKey);

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}