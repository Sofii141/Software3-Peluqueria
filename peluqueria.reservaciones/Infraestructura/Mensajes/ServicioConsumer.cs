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
    public class ServicioConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Configuración idéntica al Monolito
        private const string EXCHANGE_NAME = "servicio_exchange";

        // Escuchamos todo lo que empiece por "servicio." (creado, actualizado, etc.)
        private const string ROUTING_KEY = "servicio.#";

        public ServicioConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.servicio.queue")
        {
            _scopeFactory = scopeFactory;

            // 1. Aseguramos que el Exchange exista (debe coincidir con el del Monolito)
            _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: ExchangeType.Topic, durable: true);

            // 2. Unimos nuestra cola a ese Exchange
            _channel.QueueBind(queue: QueueName, exchange: EXCHANGE_NAME, routingKey: ROUTING_KEY);
        }

        protected override async Task ProcessMessageAsync(string message, string routingKey)
        {
            try
            {
                // Configuración para que no importen mayúsculas/minúsculas en el JSON
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1. Deserializar
                var eventoDto = JsonSerializer.Deserialize<ServicioEventoDTO>(message, options);

                if (eventoDto == null) return;

                Console.WriteLine($"[Servicio] Evento Recibido: {eventoDto.Accion} - {eventoDto.Nombre}");

                // 2. Procesar en Base de Datos
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Obtenemos el repositorio desde el contenedor de inyección de dependencias
                    var repo = scope.ServiceProvider.GetRequiredService<IServicioRepositorio>();

                    if (eventoDto.Accion.ToUpper() == "INACTIVADO")
                    {
                        // Lógica de borrado lógico
                        await repo.DeactivateAsync(eventoDto.Id);
                    }
                    else
                    {
                        // Lógica de Crear o Actualizar (Upsert)
                        // Mapeamos del DTO de entrada a la Entidad pura del Dominio
                        var servicioDominio = new Servicio
                        {
                            Id = eventoDto.Id,
                            Nombre = eventoDto.Nombre,
                            DuracionMinutos = eventoDto.DuracionMinutos,
                            Precio = eventoDto.Precio,
                            CategoriaId = eventoDto.CategoriaId,
                            Disponible = eventoDto.Disponible
                            // Nota: No necesitamos mapear 'Imagen' ni 'Descripcion' si no son relevantes para calcular reservas
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