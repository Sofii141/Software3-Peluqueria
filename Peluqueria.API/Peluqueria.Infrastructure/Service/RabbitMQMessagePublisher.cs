using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peluqueria.Application.Services
{
    public class EstilistaAgendaService : IEstilistaAgendaService
    {
        private readonly IEstilistaAgendaRepository _agendaRepo;
        private readonly IMessagePublisher _messagePublisher;

        // Constante para el Exchange de RabbitMQ
        private const string EXCHANGE_NAME = "agenda_exchange";

        public EstilistaAgendaService(IEstilistaAgendaRepository agendaRepo, IMessagePublisher messagePublisher)
        {
            _agendaRepo = agendaRepo;
            _messagePublisher = messagePublisher;
        }

        // --- HORARIO BASE ---
        public async Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios)
        {
            // Validaciones
            foreach (var h in horarios)
            {
                if (h.EsLaborable && h.HoraInicio >= h.HoraFin)
                {
                    throw new ArgumentException($"Error en {h.DiaSemana}: La hora de inicio debe ser anterior a la de fin.");
                }
            }

            var newHorarios = horarios.Select(h => new HorarioSemanalBase
            {
                EstilistaId = estilistaId,
                DiaSemana = h.DiaSemana,
                HoraInicioJornada = h.HoraInicio,
                HoraFinJornada = h.HoraFin,
                EsLaborable = h.EsLaborable
            }).ToList();

            // 1. Guardar en BD
            await _agendaRepo.UpdateHorarioBaseAsync(estilistaId, newHorarios);

            // 2. Publicar Evento de Integración
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

            // Routing Key: horario_base.actualizado
            await _messagePublisher.PublishAsync(evento, "horario_base.actualizado", EXCHANGE_NAME);

            return true;
        }

        // --- DESCANSOS FIJOS ---
        public async Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansosDto)
        {
            var validDescansos = new List<BloqueoDescansoFijoDiario>();

            foreach (var d in descansosDto)
            {
                // Validación: Consultamos si el día es laborable
                bool esLaborable = await _agendaRepo.IsDiaLaborableAsync(estilistaId, d.DiaSemana);

                if (!esLaborable) continue; // Ignoramos días no laborables

                validDescansos.Add(new BloqueoDescansoFijoDiario
                {
                    EstilistaId = estilistaId,
                    DiaSemana = d.DiaSemana,
                    HoraInicioDescanso = d.HoraInicio,
                    HoraFinDescanso = d.HoraFin,
                    Razon = "Pausa/Almuerzo"
                });
            }

            if (validDescansos.Count > 0)
            {
                // 1. Guardar en BD
                await _agendaRepo.UpdateDescansosFijosAsync(estilistaId, validDescansos);

                // 2. Publicar Evento
                var evento = new DescansoFijoActualizadoEventDto
                {
                    EstilistaId = estilistaId,
                    DescansosFijos = validDescansos.Select(d => new DiaHorarioEventDto
                    {
                        DiaSemana = d.DiaSemana,
                        HoraInicio = d.HoraInicioDescanso,
                        HoraFin = d.HoraFinDescanso,
                        EsLaborable = false // Es descanso
                    }).ToList()
                };

                await _messagePublisher.PublishAsync(evento, "descanso_fijo.actualizado", EXCHANGE_NAME);
            }

            return true;
        }

        public async Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia)
        {
            // 1. Eliminar en BD
            await _agendaRepo.DeleteDescansoFijoAsync(estilistaId, dia);

            // 2. Publicar Evento de Eliminación
            // Enviamos un objeto anónimo o un DTO simple indicando qué día se liberó
            var eventoEliminacion = new
            {
                EstilistaId = estilistaId,
                DiaSemana = dia,
                Accion = "ELIMINADO"
            };

            await _messagePublisher.PublishAsync(eventoEliminacion, "descanso_fijo.eliminado", EXCHANGE_NAME);
        }

        // --- BLOQUEOS (VACACIONES) ---
        public async Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto)
        {
            var bloqueo = new BloqueoRangoDiasLibres
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = bloqueoDto.FechaInicio.Date,
                FechaFinBloqueo = bloqueoDto.FechaFin.Date,
                Razon = bloqueoDto.Razon
            };

            // 1. Guardar
            var nuevoBloqueo = await _agendaRepo.CreateBloqueoDiasLibresAsync(bloqueo);

            // 2. Publicar Evento (Accion: CREADO)
            var evento = new BloqueoRangoDiasLibresEventDto
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = nuevoBloqueo.FechaInicioBloqueo,
                FechaFinBloqueo = nuevoBloqueo.FechaFinBloqueo,
                Accion = "CREADO"
            };

            await _messagePublisher.PublishAsync(evento, "bloqueo_dias.creado", EXCHANGE_NAME);

            return true;
        }

        public async Task<bool> UpdateBloqueoDiasLibresAsync(int estilistaId, int bloqueoId, BloqueoRangoDto dto)
        {
            var bloqueo = new BloqueoRangoDiasLibres
            {
                Id = bloqueoId,
                EstilistaId = estilistaId,
                FechaInicioBloqueo = dto.FechaInicio.Date,
                FechaFinBloqueo = dto.FechaFin.Date,
                Razon = dto.Razon
            };

            // 1. Actualizar
            var success = await _agendaRepo.UpdateBloqueoDiasLibresAsync(bloqueo);
            if (!success) return false;

            // 2. Publicar Evento (Accion: ACTUALIZADO)
            var evento = new BloqueoRangoDiasLibresEventDto
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = bloqueo.FechaInicioBloqueo,
                FechaFinBloqueo = bloqueo.FechaFinBloqueo,
                Accion = "ACTUALIZADO"
            };

            await _messagePublisher.PublishAsync(evento, "bloqueo_dias.actualizado", EXCHANGE_NAME);

            return true;
        }

        public async Task<bool> DeleteBloqueoDiasLibresAsync(int estilistaId, int bloqueoId)
        {
            // TRUCO: Antes de borrar, necesitamos saber las fechas para avisar a Reservas que libere esos días.
            // Buscamos el bloqueo en la lista actual (ya que el Repo solo devuelve bool al borrar)
            var bloqueosActuales = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            var bloqueoAEliminar = bloqueosActuales.FirstOrDefault(b => b.Id == bloqueoId);

            // 1. Eliminar en BD
            var success = await _agendaRepo.DeleteBloqueoDiasLibresAsync(bloqueoId, estilistaId);
            if (!success) return false;

            // 2. Publicar Evento (Accion: ELIMINADO)
            if (bloqueoAEliminar != null)
            {
                var evento = new BloqueoRangoDiasLibresEventDto
                {
                    EstilistaId = estilistaId,
                    // Enviamos las fechas originales para que el otro servicio sepa qué rango liberar
                    FechaInicioBloqueo = bloqueoAEliminar.FechaInicioBloqueo,
                    FechaFinBloqueo = bloqueoAEliminar.FechaFinBloqueo,
                    Accion = "ELIMINADO"
                };

                await _messagePublisher.PublishAsync(evento, "bloqueo_dias.eliminado", EXCHANGE_NAME);
            }

            return true;
        }

        public async Task<IEnumerable<HorarioDiaDto>> GetHorarioBaseAsync(int estilistaId)
        {
            var result = await _agendaRepo.GetHorarioBaseAsync(estilistaId);
            return result.Select(h => new HorarioDiaDto { DiaSemana = h.DiaSemana, HoraInicio = h.HoraInicioJornada, HoraFin = h.HoraFinJornada, EsLaborable = h.EsLaborable }).ToList();
        }

        public async Task<IEnumerable<BloqueoRangoDto>> GetBloqueosDiasLibresAsync(int estilistaId)
        {
            var result = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            return result.Select(b => new BloqueoRangoDto { FechaInicio = b.FechaInicioBloqueo, FechaFin = b.FechaFinBloqueo, Razon = b.Razon }).ToList();
        }

        public async Task<IEnumerable<HorarioDiaDto>> GetDescansosFijosAsync(int estilistaId)
        {
            var result = await _agendaRepo.GetDescansosFijosAsync(estilistaId);
            return result.Select(d => new HorarioDiaDto { DiaSemana = d.DiaSemana, HoraInicio = d.HoraInicioDescanso, HoraFin = d.HoraFinDescanso, EsLaborable = false }).ToList();
        }
    }
}