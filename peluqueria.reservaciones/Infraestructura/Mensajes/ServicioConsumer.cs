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
    @description: Consumer para procesar eventos relacionados con los servicios,
 */

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class ServicioConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private const string EXCHANGE_NAME = "servicio_exchange";

        private const string ROUTING_KEY = "servicio.#";

        public ServicioConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.servicio.queue")
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

                var eventoDto = JsonSerializer.Deserialize<ServicioEventoDTO>(message, options);

                if (eventoDto == null) return;

                Console.WriteLine($"[Servicio] Evento Recibido: {eventoDto.Accion} - {eventoDto.Nombre}");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IServicioRepositorio>();

                    if (eventoDto.Accion.ToUpper() == "INACTIVADO")
                    {
                        await repo.DeactivateAsync(eventoDto.Id);
                    }
                    else
                    {
                        var servicioDominio = new Servicio
                        {
                            Id = eventoDto.Id,
                            Nombre = eventoDto.Nombre,
                            DuracionMinutos = eventoDto.DuracionMinutos,
                            Precio = eventoDto.Precio,
                            CategoriaId = eventoDto.CategoriaId,
                            Disponible = eventoDto.Disponible
                        };

                        await repo.SaveOrUpdateAsync(servicioDominio);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Fallo al procesar mensaje de Servicio: {ex.Message}");
            }
        }
    }
}