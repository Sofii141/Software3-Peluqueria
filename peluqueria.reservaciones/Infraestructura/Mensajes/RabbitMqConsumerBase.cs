using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    // Hereda de BackgroundService para correr en segundo plano siempre
    public abstract class RabbitMqConsumerBase : BackgroundService
    {
        private readonly IConnection _connection;
        protected readonly IModel _channel;
        protected string QueueName { get; set; } // Nombre de la cola a consumir

        public RabbitMqConsumerBase(IOptions<RabbitMqOptions> rabbitMqOptions, string queueName)
        {
            QueueName = queueName;

            var options = rabbitMqOptions.Value;

            // Cconexión
            var factory = new ConnectionFactory()
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Aseguramos que la cola exista (Idempotente)
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            // Este evento se dispara cuando llega un mensaje
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                // Llamamos al método abstracto que las hijas deben implementar
                ProcessMessageAsync(message,routingKey).Wait();

                // Confirmamos a RabbitMQ que procesamos el mensaje (ACK)
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            // Empezar a consumir
            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        // Método que cada hijo utilizará para procesar mensajes
        protected abstract Task ProcessMessageAsync(string message,string routingKey);

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}