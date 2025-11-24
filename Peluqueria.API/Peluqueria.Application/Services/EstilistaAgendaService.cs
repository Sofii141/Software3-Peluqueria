using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Services
{
    public class EstilistaAgendaService : IEstilistaAgendaService
    {
        private readonly IEstilistaAgendaRepository _agendaRepo;
        private readonly IMessagePublisher _messagePublisher;

        public EstilistaAgendaService(IEstilistaAgendaRepository agendaRepo, IMessagePublisher messagePublisher)
        {
            _agendaRepo = agendaRepo;
            _messagePublisher = messagePublisher;
        }


        // PEL-HU-12: Actualizar Horario Base
        public async Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios)
        {
            // TODO: VALIDACIÓN RNI-H001. Horas de Cierre (Hora Inicio < Hora Fin)

            var newHorarios = horarios.Select(h => new HorarioSemanalBase
            {
                EstilistaId = estilistaId,
                DiaSemana = h.DiaSemana,
                HoraInicioJornada = h.HoraInicio,
                HoraFinJornada = h.HoraFin,
                EsLaborable = h.EsLaborable
            }).ToList();

            await _agendaRepo.UpdateHorarioBaseAsync(estilistaId, newHorarios);

            // Publicación del Evento
            var evento = new HorarioBaseEstilistaEventDto
            {
                EstilistaId = estilistaId,
                HorariosSemanales = newHorarios.Select(h => new DiaHorarioEventDto
                {
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicioJornada,
                    HoraFin = h.HoraFinJornada,
                    EsLaborable = h.EsLaborable
                }).ToList()
            };

            await _messagePublisher.PublishAsync(evento, "horario_base.actualizado", "agenda_exchange");

            return true;
        }

        // PEL-HU-13: Crear Bloqueo Días Libres (Vacaciones)
        public async Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto)
        {
            // TODO: VALIDACIÓN CRÍTICA (RNI-H003. Bloqueo por Cita (Estricto) - En el Microservicio de Reservas)

            var bloqueo = new BloqueoRangoDiasLibres
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = bloqueoDto.FechaInicio.Date,
                FechaFinBloqueo = bloqueoDto.FechaFin.Date,
                Razon = bloqueoDto.Razon
            };

            var nuevoBloqueo = await _agendaRepo.CreateBloqueoDiasLibresAsync(bloqueo);

            // Publicación del Evento
            var evento = new BloqueoRangoDiasLibresEventDto
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = nuevoBloqueo.FechaInicioBloqueo,
                FechaFinBloqueo = nuevoBloqueo.FechaFinBloqueo,
                Accion = "CREADO"
            };
            await _messagePublisher.PublishAsync(evento, "bloqueo_dias.creado", "agenda_exchange");

            return true;
        }

        // PEL-HU-13: Actualizar Descansos Fijos Diarios (Almuerzo)
        public async Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansosDto)
        {
            // TODO: VALIDACIÓN RNI-H004: Única Pausa Diaria (si aplica)

            var newDescansos = descansosDto.Select(d => new BloqueoDescansoFijoDiario
            {
                EstilistaId = estilistaId,
                DiaSemana = d.DiaSemana,
                HoraInicioDescanso = d.HoraInicio,
                HoraFinDescanso = d.HoraFin,
                Razon = "Pausa/Almuerzo"
            }).ToList();

            await _agendaRepo.UpdateDescansosFijosAsync(estilistaId, newDescansos);

            var evento = new DescansoFijoActualizadoEventDto
            {
                EstilistaId = estilistaId,
                DescansosFijos = newDescansos.Select(d => new DiaHorarioEventDto
                {
                    DiaSemana = d.DiaSemana,
                    HoraInicio = d.HoraInicioDescanso,
                    HoraFin = d.HoraFinDescanso,
                    EsLaborable = false // Es un descanso, por lo tanto, no laborable
                }).ToList()
            };

            await _messagePublisher.PublishAsync(evento, "descanso_fijo.actualizado", "agenda_exchange");

            return true;
        }

        public async Task<IEnumerable<HorarioDiaDto>> GetHorarioBaseAsync(int estilistaId)
        {
            var horarios = await _agendaRepo.GetHorarioBaseAsync(estilistaId);

            // Mapeo a DTO
            return horarios.Select(h => new HorarioDiaDto
            {
                DiaSemana = h.DiaSemana,
                HoraInicio = h.HoraInicioJornada,
                HoraFin = h.HoraFinJornada,
                EsLaborable = h.EsLaborable
            }).ToList();
        }

        // Implementación de Get Bloqueos Días Libres (PEL-HU-13)
        public async Task<IEnumerable<BloqueoRangoDto>> GetBloqueosDiasLibresAsync(int estilistaId)
        {
            var bloqueos = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);

            // Mapeo a DTO
            return bloqueos.Select(b => new BloqueoRangoDto
            {
                FechaInicio = b.FechaInicioBloqueo,
                FechaFin = b.FechaFinBloqueo,
                Razon = b.Razon
            }).ToList();
        }

        // Implementación de Get Descansos Fijos (PEL-HU-13)
        public async Task<IEnumerable<HorarioDiaDto>> GetDescansosFijosAsync(int estilistaId)
        {
            var descansos = await _agendaRepo.GetDescansosFijosAsync(estilistaId);

            // Mapeo a DTO (reutilizando HorarioDiaDto, marcando como no laborable)
            return descansos.Select(d => new HorarioDiaDto
            {
                DiaSemana = d.DiaSemana,
                HoraInicio = d.HoraInicioDescanso,
                HoraFin = d.HoraFinDescanso,
                EsLaborable = false
            }).ToList();
        }
    }
}
