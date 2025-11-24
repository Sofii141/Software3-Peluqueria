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

        public EstilistaAgendaService(IEstilistaAgendaRepository agendaRepo, IMessagePublisher messagePublisher)
        {
            _agendaRepo = agendaRepo;
            _messagePublisher = messagePublisher;
        }

        // --- HORARIO BASE ---
        public async Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios)
        {
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

            await _agendaRepo.UpdateHorarioBaseAsync(estilistaId, newHorarios);

            // TODO: Publicar evento 'horario_base.actualizado'
            return true;
        }

        // --- DESCANSOS FIJOS ---
        public async Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansosDto)
        {
            var validDescansos = new List<BloqueoDescansoFijoDiario>();

            foreach (var d in descansosDto)
            {
                // VALIDACIÓN DE NEGOCIO:
                // Consultamos al repo si ese día el estilista trabaja.
                bool esLaborable = await _agendaRepo.IsDiaLaborableAsync(estilistaId, d.DiaSemana);

                if (!esLaborable)
                {
                    // Si el día NO es laborable, IGNORAMOS este descanso.
                    // No lanzamos error para no romper la operación masiva, simplemente no se guarda.
                    continue;
                }

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
                await _agendaRepo.UpdateDescansosFijosAsync(estilistaId, validDescansos);
                // TODO: Publicar evento 'descanso_fijo.actualizado'
            }

            return true;
        }

        public async Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia)
        {
            await _agendaRepo.DeleteDescansoFijoAsync(estilistaId, dia);
        }

        // --- BLOQUEOS ---
        public async Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto)
        {
            var bloqueo = new BloqueoRangoDiasLibres
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = bloqueoDto.FechaInicio.Date,
                FechaFinBloqueo = bloqueoDto.FechaFin.Date,
                Razon = bloqueoDto.Razon
            };

            await _agendaRepo.CreateBloqueoDiasLibresAsync(bloqueo);
            // TODO: Publicar evento 'bloqueo.creado'
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

            return await _agendaRepo.UpdateBloqueoDiasLibresAsync(bloqueo);
        }

        public async Task<bool> DeleteBloqueoDiasLibresAsync(int estilistaId, int bloqueoId)
        {
            return await _agendaRepo.DeleteBloqueoDiasLibresAsync(bloqueoId, estilistaId);
        }

        // --- GETTERS ---
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