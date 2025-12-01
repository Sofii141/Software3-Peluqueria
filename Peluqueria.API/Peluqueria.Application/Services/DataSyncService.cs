using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Application.Services
{
    public class DataSyncService : IDataSyncService
    {
        private readonly IEstilistaRepository _estilistaRepo;
        private readonly IServicioRepository _servicioRepo;
        private readonly ICategoriaRepository _categoriaRepo;
        // Agregamos el repo de Agenda para leer horarios
        private readonly IEstilistaAgendaRepository _agendaRepo;
        private readonly IMessagePublisher _messagePublisher;

        public DataSyncService(
            IEstilistaRepository estilistaRepo,
            IServicioRepository servicioRepo,
            ICategoriaRepository categoriaRepo,
            IEstilistaAgendaRepository agendaRepo, // <--- INYECTAR
            IMessagePublisher messagePublisher)
        {
            _estilistaRepo = estilistaRepo;
            _servicioRepo = servicioRepo;
            _categoriaRepo = categoriaRepo;
            _agendaRepo = agendaRepo;
            _messagePublisher = messagePublisher;
        }

        public async Task SincronizarTodoAsync()
        {
            // 1. Sincronizar Categorías
            var categorias = await _categoriaRepo.GetAllAsync();
            foreach (var c in categorias)
            {
                var evento = new CategoriaEventDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    EstaActiva = c.EstaActiva,
                    Accion = "CREADA" // Upsert
                };
                await _messagePublisher.PublishAsync(evento, "categoria.sincronizacion", "categoria_exchange");
            }

            // 2. Sincronizar Servicios
            var servicios = await _servicioRepo.GetAllAsync();
            foreach (var s in servicios)
            {
                var evento = new ServicioEventDto
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    DuracionMinutos = s.DuracionMinutos,
                    Precio = s.Precio,
                    CategoriaId = s.CategoriaId,
                    Disponible = s.Disponible,
                    Accion = "CREADA"
                };
                await _messagePublisher.PublishAsync(evento, "servicio.sincronizacion", "servicio_exchange");
            }

            // 3. Sincronizar Estilistas y su AGENDA
            var estilistas = await _estilistaRepo.GetAllAsync();
            foreach (var e in estilistas)
            {
                // A. Enviar Estilista
                var eventoEstilista = new EstilistaEventDto
                {
                    Id = e.Id,
                    IdentityId = e.IdentityId,
                    NombreCompleto = e.NombreCompleto,
                    Telefono = e.Telefono,
                    EstaActivo = e.EstaActivo,
                    ImagenUrl = e.Imagen,
                    Accion = "CREADO",
                    ServiciosAsociados = e.ServiciosAsociados.Select(es => new EstilistaServicioMinimalEventDto
                    {
                        ServicioId = es.ServicioId,
                        DuracionMinutos = es.Servicio?.DuracionMinutos ?? 0
                    }).ToList()
                };
                await _messagePublisher.PublishAsync(eventoEstilista, "estilista.sincronizacion", "estilista_exchange");

                // B. Sincronizar Horario Base del Estilista
                var horarioBase = await _agendaRepo.GetHorarioBaseAsync(e.Id);
                if (horarioBase.Any())
                {
                    var eventoHorario = new HorarioBaseEstilistaEventDto
                    {
                        EstilistaId = e.Id,
                        HorariosSemanales = horarioBase.Select(h => new DiaHorarioEventDto
                        {
                            DiaSemana = h.DiaSemana,
                            HoraInicio = h.HoraInicioJornada,
                            HoraFin = h.HoraFinJornada,
                            EsLaborable = h.EsLaborable
                        }).ToList()
                    };
                    // Enviamos al exchange de Agenda
                    await _messagePublisher.PublishAsync(eventoHorario, "horario_base.sincronizacion", "agenda_exchange");
                }

                // C. Sincronizar Descansos Fijos
                var descansos = await _agendaRepo.GetDescansosFijosAsync(e.Id);
                if (descansos.Any())
                {
                    var eventoDescanso = new DescansoFijoActualizadoEventDto
                    {
                        EstilistaId = e.Id,
                        DescansosFijos = descansos.Select(d => new DiaHorarioEventDto
                        {
                            DiaSemana = d.DiaSemana,
                            HoraInicio = d.HoraInicioDescanso,
                            HoraFin = d.HoraFinDescanso,
                            EsLaborable = false
                        }).ToList()
                    };
                    await _messagePublisher.PublishAsync(eventoDescanso, "descanso_fijo.sincronizacion", "agenda_exchange");
                }

                // D. Sincronizar Bloqueos (Vacaciones)
                var bloqueos = await _agendaRepo.GetBloqueosDiasLibresAsync(e.Id);
                foreach (var b in bloqueos)
                {
                    var eventoBloqueo = new BloqueoRangoDiasLibresEventDto
                    {
                        EstilistaId = e.Id,
                        FechaInicioBloqueo = b.FechaInicioBloqueo,
                        FechaFinBloqueo = b.FechaFinBloqueo,
                        Accion = "CREADO"
                    };
                    await _messagePublisher.PublishAsync(eventoBloqueo, "bloqueo_dias.sincronizacion", "agenda_exchange");
                }
            }
        }
    }
}