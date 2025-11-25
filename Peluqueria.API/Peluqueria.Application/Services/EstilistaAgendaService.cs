using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Exceptions;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Services
{
    public class EstilistaAgendaService : IEstilistaAgendaService
    {
        private readonly IEstilistaAgendaRepository _agendaRepo;
        private readonly IEstilistaRepository _estilistaRepo;
        private readonly IMessagePublisher _messagePublisher;

        private const string EXCHANGE_NAME = "agenda_exchange";

        public EstilistaAgendaService(IEstilistaAgendaRepository agendaRepo,
                                      IEstilistaRepository estilistaRepo,
                                      IMessagePublisher messagePublisher)
        {
            _agendaRepo = agendaRepo;
            _estilistaRepo = estilistaRepo;
            _messagePublisher = messagePublisher;
        }

        // --- HORARIO BASE ---
        public async Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios)
        {
            // 1. Validar existencia del Estilista
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            // 2. Validar Coherencia (Inicio < Fin)
            foreach (var h in horarios)
            {
                if (h.EsLaborable && h.HoraInicio >= h.HoraFin)
                {
                    throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO,
                        $"Error en {h.DiaSemana}: La hora de inicio debe ser anterior a la de fin.");
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

            // Publicar Evento
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
            await _messagePublisher.PublishAsync(evento, "horario_base.actualizado", EXCHANGE_NAME);

            return true;
        }

        // --- DESCANSOS FIJOS (Corrección "Doble Click") ---
        public async Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansosDto)
        {
            // 1. Validar Estilista
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            var validDescansos = new List<BloqueoDescansoFijoDiario>();

            foreach (var d in descansosDto)
            {
                // 2. Validar lógica de horas
                if (d.HoraInicio >= d.HoraFin)
                    throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, $"Error en {d.DiaSemana}: Inicio del descanso debe ser antes del fin.");

                // 3. Validar si el día es laborable (CORREGIDO)
                // Antes: if (!esLaborable) continue; (Esto ocultaba el error)
                // Ahora: Lanzamos excepción si intentan poner descanso en día no configurado/no laborable.
                bool esLaborable = await _agendaRepo.IsDiaLaborableAsync(estilistaId, d.DiaSemana);

                if (!esLaborable)
                {
                    throw new ReglaNegocioException(CodigoError.REGLA_NEGOCIO_VIOLADA,
                        $"No se puede asignar descanso el {d.DiaSemana} porque no está configurado como laborable en el Horario Base.");
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

                var evento = new DescansoFijoActualizadoEventDto
                {
                    EstilistaId = estilistaId,
                    DescansosFijos = validDescansos.Select(d => new DiaHorarioEventDto
                    {
                        DiaSemana = d.DiaSemana,
                        HoraInicio = d.HoraInicioDescanso,
                        HoraFin = d.HoraFinDescanso,
                        EsLaborable = false
                    }).ToList()
                };
                await _messagePublisher.PublishAsync(evento, "descanso_fijo.actualizado", EXCHANGE_NAME);
            }

            return true;
        }

        // --- BLOQUEOS (VACACIONES) ---
        public async Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto)
        {
            // 1. Validar Estilista
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            // 2. Validar Fechas
            if (bloqueoDto.FechaInicio > bloqueoDto.FechaFin)
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "La fecha de inicio debe ser anterior o igual a la fecha de fin.");

            if (bloqueoDto.FechaInicio.Date < DateTime.UtcNow.Date)
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "No se pueden bloquear fechas en el pasado.");

            // 3. Validar Superposición (Solapamiento)
            var bloqueosExistentes = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            bool haySolapamiento = bloqueosExistentes.Any(b =>
                bloqueoDto.FechaInicio.Date <= b.FechaFinBloqueo.Date &&
                bloqueoDto.FechaFin.Date >= b.FechaInicioBloqueo.Date
            );

            if (haySolapamiento)
                throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, "El rango seleccionado choca con otro bloqueo existente.");

            // 4. Validar Citas (Simulado)
            // if (tieneCitas) throw ...

            var bloqueo = new BloqueoRangoDiasLibres
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = bloqueoDto.FechaInicio.Date,
                FechaFinBloqueo = bloqueoDto.FechaFin.Date,
                Razon = bloqueoDto.Razon
            };

            var nuevoBloqueo = await _agendaRepo.CreateBloqueoDiasLibresAsync(bloqueo);

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
            // 1. Validar Estilista PRIMERO (Esto arregla tu error 404 confuso)
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            // 2. Validar Fechas
            if (dto.FechaInicio > dto.FechaFin)
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "Fechas incoherentes.");

            // 3. Validar Superposición (Excluyendo el propio ID)
            var bloqueosExistentes = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            bool haySolapamiento = bloqueosExistentes.Any(b =>
                b.Id != bloqueoId && // Ignorar el que estoy editando
                dto.FechaInicio.Date <= b.FechaFinBloqueo.Date &&
                dto.FechaFin.Date >= b.FechaInicioBloqueo.Date
            );

            if (haySolapamiento)
                throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, "El rango seleccionado choca con otro bloqueo existente.");

            // 4. Actualizar
            var bloqueo = new BloqueoRangoDiasLibres
            {
                Id = bloqueoId,
                EstilistaId = estilistaId,
                FechaInicioBloqueo = dto.FechaInicio.Date,
                FechaFinBloqueo = dto.FechaFin.Date,
                Razon = dto.Razon
            };

            var success = await _agendaRepo.UpdateBloqueoDiasLibresAsync(bloqueo);

            // Si el repo devuelve false, significa que el ID del bloqueo no existe o no es de este estilista
            if (!success)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA, "El bloqueo no existe o no pertenece al estilista.");

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
            // 1. Validar Estilista PRIMERO
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            // 2. Buscar Bloqueo específico (Para tener datos para el evento)
            var bloqueosActuales = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            var bloqueoAEliminar = bloqueosActuales.FirstOrDefault(b => b.Id == bloqueoId);

            if (bloqueoAEliminar == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA, "El bloqueo no existe.");

            // 3. Borrar
            await _agendaRepo.DeleteBloqueoDiasLibresAsync(bloqueoId, estilistaId);

            var evento = new BloqueoRangoDiasLibresEventDto
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = bloqueoAEliminar.FechaInicioBloqueo,
                FechaFinBloqueo = bloqueoAEliminar.FechaFinBloqueo,
                Accion = "ELIMINADO"
            };
            await _messagePublisher.PublishAsync(evento, "bloqueo_dias.eliminado", EXCHANGE_NAME);

            return true;
        }

        public async Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia)
        {
            // Validar Estilista
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            await _agendaRepo.DeleteDescansoFijoAsync(estilistaId, dia);

            var eventoEliminacion = new
            {
                EstilistaId = estilistaId,
                DiaSemana = dia,
                Accion = "ELIMINADO"
            };
            await _messagePublisher.PublishAsync(eventoEliminacion, "descanso_fijo.eliminado", EXCHANGE_NAME);
        }

        // --- GETTERS (Validando Estilista) ---

        public async Task<IEnumerable<HorarioDiaDto>> GetHorarioBaseAsync(int estilistaId)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            var result = await _agendaRepo.GetHorarioBaseAsync(estilistaId);
            return result.Select(h => new HorarioDiaDto { DiaSemana = h.DiaSemana, HoraInicio = h.HoraInicioJornada, HoraFin = h.HoraFinJornada, EsLaborable = h.EsLaborable }).ToList();
        }

        public async Task<IEnumerable<BloqueoResponseDto>> GetBloqueosDiasLibresAsync(int estilistaId)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            var result = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);

            return result.Select(b => new BloqueoResponseDto
            {
                Id = b.Id, // Aquí se devuelve el ID correcto (ej: 8)
                FechaInicio = b.FechaInicioBloqueo,
                FechaFin = b.FechaFinBloqueo,
                Razon = b.Razon
            }).ToList();
        }

        public async Task<IEnumerable<HorarioDiaDto>> GetDescansosFijosAsync(int estilistaId)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            var result = await _agendaRepo.GetDescansosFijosAsync(estilistaId);
            return result.Select(d => new HorarioDiaDto { DiaSemana = d.DiaSemana, HoraInicio = d.HoraInicioDescanso, HoraFin = d.HoraFinDescanso, EsLaborable = false }).ToList();
        }
    }
}