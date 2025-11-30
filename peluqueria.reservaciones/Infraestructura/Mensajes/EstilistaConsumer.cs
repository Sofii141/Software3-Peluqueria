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
    @description: Consumer para procesar eventos relacionados con los estilistas,
 */

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class EstilistaConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private const string EXCHANGE_NAME = "estilista_exchange";
        private const string ROUTING_KEY = "estilista.#"; 

        public EstilistaConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.estilista.queue")
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
                var eventoDto = JsonSerializer.Deserialize<EstilistaEventoDTO>(message, options);

                if (eventoDto == null) return;

                Console.WriteLine($"[Estilista] Evento Recibido: {eventoDto.Accion} - {eventoDto.NombreCompleto}");

                using (var scope = _scopeFactory.CreateScope())
                {
                    // Obtenemos la interfaz de repositorio
                    var repo = scope.ServiceProvider.GetRequiredService<IEstilistaRepositorio>();

                    if (eventoDto.Accion.ToUpper() == "INACTIVADO")
                    {
                        await repo.DeactivateAsync(eventoDto.Id);
                    }
                    else
                    {

                        var estilistaDominio = new Estilista
                        {
                            Id = eventoDto.Id,
                            Identificacion = eventoDto.IdentityId,
                            NombreCompleto = eventoDto.NombreCompleto,
                            EstaActivo = eventoDto.EstaActivo
                        };

                        await repo.SaveOrUpdateAsync(estilistaDominio);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Fallo al procesar mensaje de Estilista: {ex.Message}");
            }
        }
    }
}