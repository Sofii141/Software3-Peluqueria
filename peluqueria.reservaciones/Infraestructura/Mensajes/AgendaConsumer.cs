using System; 
using System.Linq; 
using System.Text.Json;
using System.Threading.Tasks; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.DTO.Eventos;

/*
 @author: Juan david Moran
    @description: Consumer para procesar eventos relacionados con los horarios de los estilistas,
 */

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class AgendaConsumer : RabbitMqConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private const string EXCHANGE_NAME = "agenda_exchange";
        private const string ROUTING_KEY = "#";

        public AgendaConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
            : base(rabbitMqOptions, "hair.reservations.agenda.queue")
        {
            _scopeFactory = scopeFactory;
            _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: ExchangeType.Topic, durable: true);
            _channel.QueueBind(queue: QueueName, exchange: EXCHANGE_NAME, routingKey: ROUTING_KEY);
        }

        protected override async Task ProcessMessageAsync(string message, string routingKey)
        {
            try
            {
                Console.WriteLine($"[Agenda] Evento Recibido: {routingKey}");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IHorarioRepositorio>();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    if (routingKey.StartsWith("horario_base."))
                    {
                        var dto = JsonSerializer.Deserialize<HorarioEventoDTO>(message, options);
                        if (dto == null) return;

                        var horarioBaseDominio = new HorarioBase
                        {
                            EstilistaId = dto.EstilistaId,
                            HorariosSemanales = dto.HorariosSemanales.Select(h => new DiaHorario
                            {
                                DiaSemana = h.DiaSemana,
                                HoraInicio = h.HoraInicio,
                                HoraFin = h.HoraFin,
                                EsLaborable = h.EsLaborable
                            }).ToList()
                        };

                        await repo.SetBaseScheduleAsync(horarioBaseDominio.EstilistaId, horarioBaseDominio.HorariosSemanales);
                    }
                    else if (routingKey.StartsWith("descanso_fijo."))
                    {
                        var dto = JsonSerializer.Deserialize<DescansoEventoDTO>(message, options);
                        if (dto == null) return;

                        var descansoFijoDominio = new DescansoFijo
                        {
                            EstilistaId = dto.EstilistaId,
                            DescansosFijos = dto.DescansosFijos.Select(d => new DiaHorario
                            {
                                DiaSemana = d.DiaSemana,
                                HoraInicio = d.HoraInicio,
                                HoraFin = d.HoraFin,
                                EsLaborable = false
                            }).ToList()
                        };

                        await repo.SetFixedBreaksAsync(descansoFijoDominio.EstilistaId, descansoFijoDominio.DescansosFijos);
                    }
                    else if (routingKey.StartsWith("bloqueo_dias."))
                    {
                        var dto = JsonSerializer.Deserialize<RangoLibreEventoDTO>(message, options);
                        if (dto == null) return;

                        var bloqueo = new BloqueoRangoDiasLibres
                        {
                            EstilistaId = dto.EstilistaId,
                            FechaInicioBloqueo = dto.FechaInicioBloqueo,
                            FechaFinBloqueo = dto.FechaFinBloqueo,
                            Accion = dto.Accion
                        };

                        if (bloqueo.Accion != null && bloqueo.Accion.ToUpper() == "ELIMINADO")
                        {
                            
                        }
                        else 
                        {
                            await repo.AddBlockoutRangeAsync(bloqueo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Fallo al procesar mensaje de Agenda ({routingKey}): {ex.Message}");
            }
        }
    }
}