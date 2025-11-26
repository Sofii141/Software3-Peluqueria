using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using peluqueria.reservaciones.Infraestructura.DTO.Eventos;
using peluqueria.reservaciones.Core.Dominio;      
using peluqueria.reservaciones.Core.Puertos.Salida; 
using peluqueria.reservaciones.Infraestructura.Persistencia;
using Microsoft.Extensions.Options;

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class ClienteConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private const string EXCHANGE_NAME = "cliente_exchange";

        // Escuchamos todos los eventos que empiecen por "cliente."
        private const string ROUTING_KEY = "cliente.#";

        public ClienteConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.cliente.queue")
        {
            _scopeFactory = scopeFactory;

            _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: ExchangeType.Topic, durable: true);
            _channel.QueueBind(queue: QueueName, exchange: EXCHANGE_NAME, routingKey: ROUTING_KEY);
        }

        // Usamos la nueva firma del método para mantener la coherencia con la Clase Base
        protected override async Task ProcessMessageAsync(string message, string routingKey)
        {
            try
            {
                if (routingKey != "cliente.registrado") return; // Solo procesamos el evento de registro

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var eventoDto = JsonSerializer.Deserialize<ClienteEventoDTO>(message, options);

                if (eventoDto == null) return;

                Console.WriteLine($"[Cliente] Evento Recibido: REGISTRADO - {eventoDto.Username}");

                using (var scope = _scopeFactory.CreateScope())
                {
                    // Obtenemos el repositorio
                    var repo = scope.ServiceProvider.GetRequiredService<IClienteRepositorio>();

                    // Mapeamos al Dominio
                    var clienteDominio = new Cliente
                    {
                        // Usamos IdentityId del monolito como Identificacion en el microservicio
                        Identificacion = eventoDto.IdentityId,
                        NombreCompleto = eventoDto.NombreCompleto,
                        NombreUsuario = eventoDto.Username
                    };

                    // Guardamos el cliente (esencialmente un 'Create' o 'Upsert' si el cliente ya existiera)
                    await repo.SaveOrUpdateAsync(clienteDominio);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Fallo al procesar mensaje de Cliente: {ex.Message}");
            }
        }
    }
}