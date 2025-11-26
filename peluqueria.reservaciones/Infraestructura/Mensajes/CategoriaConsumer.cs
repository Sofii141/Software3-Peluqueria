using peluqueria.reservaciones.Infraestructura.DTO.Eventos;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Infraestructura.Persistencia;
using peluqueria.reservaciones.Core.Puertos.Salida;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class CategoriaConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private const string EXCHANGE_NAME = "categoria_exchange";

        // Routing Key con comodín (#) para escuchar: categoria.creada, categoria.actualizada, etc.
        private const string ROUTING_KEY = "categoria.#";

        public CategoriaConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.categoria.queue")
        {
            _scopeFactory = scopeFactory;

            // IMPORTANTE: Declaramos el Exchange (por seguridad, para asegurar que existe)
            // Debe coincidir en tipo (Topic) con el del monolito.
            _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: ExchangeType.Topic, durable: true);

            // IMPORTANTE: Unimos (Bind) nuestra cola al Exchange del monolito
            _channel.QueueBind(queue: QueueName, exchange: EXCHANGE_NAME, routingKey: ROUTING_KEY);
        }

        protected override async Task ProcessMessageAsync(string message, string routingKey)
        {
            try
            {
                // 1. Deserializar el JSON al DTO de Entrada
                // Usamos CaseInsensitive para que no importe si envían "Id" o "id"
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var eventoDto = JsonSerializer.Deserialize<CategoriaEventoDTO>(message, options);

                if (eventoDto == null) return;

                Console.WriteLine($"[Categoria] Evento Recibido: {eventoDto.Accion} - {eventoDto.Nombre}");

                // 2. Procesar según la acción
                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<ICategoriaRepositorio>();

                    if (eventoDto.Accion.ToUpper() == "INACTIVADA")
                    {
                        // Caso: Inactivación lógica
                        await repo.DeactivateAsync(eventoDto.Id);
                    }
                    else
                    {
                        // Caso: CREADA o ACTUALIZADA (Upsert)
                        // Mapeamos del DTO a la Entidad de Dominio
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