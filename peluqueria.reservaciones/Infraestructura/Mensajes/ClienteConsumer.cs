using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using peluqueria.reservaciones.Infraestructura.DTO.Eventos;
using peluqueria.reservaciones.Core.Dominio;      
using peluqueria.reservaciones.Core.Puertos.Salida; 
using peluqueria.reservaciones.Infraestructura.Persistencia;
using Microsoft.Extensions.Options;

/*
 @author: Juan David Moran
    @description: Consumer para procesar eventos relacionados con los clientes,
 */

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class ClienteConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private const string EXCHANGE_NAME = "cliente_exchange";

        private const string ROUTING_KEY = "cliente.#";

        public ClienteConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.cliente.queue")
        {
            _scopeFactory = scopeFactory;

            _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: ExchangeType.Topic, durable: true);
            _channel.QueueBind(queue: QueueName, exchange: EXCHANGE_NAME, routingKey: ROUTING_KEY);
        }

        protected override async Task ProcessMessageAsync(string message, string routingKey)
        {
            try
            {
                if (routingKey != "cliente.registrado") return; 

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var eventoDto = JsonSerializer.Deserialize<ClienteEventoDTO>(message, options);

                if (eventoDto == null) return;

                Console.WriteLine($"[Cliente] Evento Recibido: REGISTRADO - {eventoDto.Username}");

                using (var scope = _scopeFactory.CreateScope())
                {
                    
                    var repo = scope.ServiceProvider.GetRequiredService<IClienteRepositorio>();

                   
                    var clienteDominio = new Cliente
                    {
                        Identificacion = eventoDto.IdentityId,
                        NombreCompleto = eventoDto.NombreCompleto,
                        NombreUsuario = eventoDto.Username
                    };

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