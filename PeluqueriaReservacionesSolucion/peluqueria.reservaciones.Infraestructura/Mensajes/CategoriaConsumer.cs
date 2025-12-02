using peluqueria.reservaciones.Infraestructura.Eventos;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Infraestructura.Persistencia;
using peluqueria.reservaciones.Core.Puertos.Salida;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Text.Json;
using Microsoft.Extensions.Options;

/*
 @author: Juan David Moran
    @description: Consumer para procesar eventos relacionados con las categorías de servicios,
 */

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class CategoriaConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private const string EXCHANGE_NAME = "categoria_exchange";

        private const string ROUTING_KEY = "categoria.#";

        public CategoriaConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.categoria.queue")
        {
            _scopeFactory = scopeFactory;

            _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: ExchangeType.Topic, durable: true);

            _channel.QueueBind(queue: QueueName, exchange: EXCHANGE_NAME, routingKey: ROUTING_KEY);
        }

        protected override async Task ProcessMessageAsync(string message, string routingKey)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var eventoDto = JsonSerializer.Deserialize<CategoriaEventoDTO>(message, options);

                if (eventoDto == null) return;

                Console.WriteLine($"[Categoria] Evento Recibido: {eventoDto.Accion} - {eventoDto.Nombre}");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<ICategoriaRepositorio>();

                    if (eventoDto.Accion.ToUpper() == "INACTIVADA")
                    {
                        await repo.DeactivateAsync(eventoDto.Id);
                    }
                    else
                    {
                        var categoriaDominio = new Categoria
                        {
                            Id = eventoDto.Id,
                            Nombre = eventoDto.Nombre,
                            EstaActiva = eventoDto.EstaActiva
                        };

                        await repo.SaveOrUpdateAsync(categoriaDominio);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Fallo al procesar mensaje de Categoria: {ex.Message}");
            }
        }
    }
}