using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Exceptions;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Services
{
    /// <summary>
    /// Servicio de lógica compleja para la gestión de horarios y agenda.
    /// </summary>
    /// <remarks>
    /// Este servicio consulta intensivamente al Microservicio de Reservas para evitar 
    /// corrupciones de datos (ej. quitar un día laborable cuando ya hay citas vendidas).
    /// </remarks>
    public class EstilistaAgendaService : IEstilistaAgendaService
    {
        private readonly IEstilistaAgendaRepository _agendaRepo;
        private readonly IEstilistaRepository _estilistaRepo;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IReservacionClient _reservacionClient;

        private const string EXCHANGE_NAME = "agenda_exchange";

        public EstilistaAgendaService(IEstilistaAgendaRepository agendaRepo,
                                      IEstilistaRepository estilistaRepo,
                                      IMessagePublisher messagePublisher,
                                      IReservacionClient reservacionClient)
        {
            _agendaRepo = agendaRepo;
            _estilistaRepo = estilistaRepo;
            _messagePublisher = messagePublisher;
            _reservacionClient = reservacionClient;
        }

        // --- 1. ACTUALIZAR HORARIO BASE (Jornada Laboral) ---
        /// <summary>
        /// Actualiza la configuración semanal (Lunes-Domingo).
        /// </summary>
        /// <exception cref="ReglaNegocioException">Si se intenta reducir jornada y quedan citas por fuera.</exception>
        public async Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            foreach (var h in horarios)
            {
                // Validación básica: Inicio < Fin
                if (h.EsLaborable && h.HoraInicio >= h.HoraFin)
                {
                    throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO,
                        $"Error en {h.DiaSemana}: La hora de inicio debe ser anterior a la de fin.");
                }

                // CASO A: Marcar día como NO LABORABLE (Día Libre)
                if (!h.EsLaborable)
                {
                    bool conflicto = await _reservacionClient.TieneReservasEnDia(estilistaId, h.DiaSemana);
                    if (conflicto)
                    {
                        throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS,
                            $"No se puede marcar el {h.DiaSemana} como no laborable porque ya existen citas agendadas.");
                    }
                }
                // CASO B: Reducción de Jornada (Si recortan horas, validar que no queden citas fuera)
                else
                {
                    // Chequeamos si hay citas antes de la nueva hora de entrada (00:00 -> NuevaHoraInicio)
                    bool conflictoManana = await _reservacionClient.TieneReservasEnDescanso(
                        estilistaId, h.DiaSemana, TimeSpan.Zero, h.HoraInicio);

                    // Chequeamos si hay citas después de la nueva hora de salida (NuevaHoraFin -> 23:59)
                    bool conflictoTarde = await _reservacionClient.TieneReservasEnDescanso(
                        estilistaId, h.DiaSemana, h.HoraFin, new TimeSpan(23, 59, 0));

                    if (conflictoManana || conflictoTarde)
                    {
                        throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS,
                            $"El cambio de horario en {h.DiaSemana} deja citas existentes fuera de la jornada laboral.");
                    }
                }
            }

            // Mapeo y Guardado
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

        // --- 2. ACTUALIZAR DESCANSOS FIJOS (Hora de Almuerzo) ---
        public async Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansosDto)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            var validDescansos = new List<BloqueoDescansoFijoDiario>();

            foreach (var d in descansosDto)
            {
                if (d.HoraInicio >= d.HoraFin)
                    throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, $"Error en {d.DiaSemana}: Inicio antes del fin.");

                // Validar si es día laborable
                bool esLaborable = await _agendaRepo.IsDiaLaborableAsync(estilistaId, d.DiaSemana);
                if (!esLaborable)
                {
                    throw new ReglaNegocioException(CodigoError.REGLA_NEGOCIO_VIOLADA,
                        $"No se puede asignar descanso el {d.DiaSemana} porque no es laborable.");
                }

                // --- VALIDACIÓN MICROSERVICIO ---
                // ¿Hay citas justo en esa hora de descanso?
                bool conflicto = await _reservacionClient.TieneReservasEnDescanso(
                    estilistaId, d.DiaSemana, d.HoraInicio, d.HoraFin);

                if (conflicto)
                {
                    throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS,
                        $"No se puede agregar el descanso del {d.DiaSemana} ({d.HoraInicio}-{d.HoraFin}) porque choca con citas existentes.");
                }
                // --------------------------------

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

        // --- 3. CREAR BLOQUEO (Vacaciones) ---
        /// <summary>
        /// Bloquea un rango de fechas (Vacaciones).
        /// </summary>
        /// <remarks>
        /// Valida solapamientos locales y consulta al microservicio si existen citas en ese periodo.
        /// </remarks>
        public async Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            // Validaciones de fechas
            if (bloqueoDto.FechaInicio > bloqueoDto.FechaFin)
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "Fechas incoherentes.");
            if (bloqueoDto.FechaInicio.Date < DateTime.UtcNow.Date)
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "No se pueden bloquear fechas pasadas.");

            // Validar solapamiento local (con otros bloqueos)
            var bloqueosExistentes = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            bool haySolapamiento = bloqueosExistentes.Any(b =>
                bloqueoDto.FechaInicio.Date <= b.FechaFinBloqueo.Date &&
                bloqueoDto.FechaFin.Date >= b.FechaInicioBloqueo.Date
            );
            if (haySolapamiento)
                throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, "Choca con otro bloqueo existente.");

            // --- VALIDACIÓN MICROSERVICIO ---
            // ¿Hay citas en ese rango de vacaciones?
            bool tieneCitas = await _reservacionClient.TieneReservasEnRango(
                estilistaId,
                DateOnly.FromDateTime(bloqueoDto.FechaInicio),
                DateOnly.FromDateTime(bloqueoDto.FechaFin)
            );

            if (tieneCitas)
            {
                throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS,
                    "No se puede bloquear este rango de fechas porque existen reservaciones agendadas.");
            }
            // --------------------------------

            var bloqueo = new BloqueoRangoDiasLibres
            {
                EstilistaId = estilistaId,
                FechaInicioBloqueo = bloqueoDto.FechaInicio.Date,
                FechaFinBloqueo = bloqueoDto.FechaFin.Date,
                Razon = bloqueoDto.Razon
            };

            var nuevoBloqueo = await _agendaRepo.CreateBloqueoDiasLibresAsync(bloqueo);

            // Publicar evento...
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

        // --- 4. ACTUALIZAR BLOQUEO ---
        public async Task<bool> UpdateBloqueoDiasLibresAsync(int estilistaId, int bloqueoId, BloqueoRangoDto dto)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            if (dto.FechaInicio > dto.FechaFin)
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "Fechas incoherentes.");

            // Validar solapamiento local
            var bloqueosExistentes = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            bool haySolapamiento = bloqueosExistentes.Any(b =>
                b.Id != bloqueoId &&
                dto.FechaInicio.Date <= b.FechaFinBloqueo.Date &&
                dto.FechaFin.Date >= b.FechaInicioBloqueo.Date
            );
            if (haySolapamiento)
                throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, "Choca con otro bloqueo existente.");

            // --- VALIDACIÓN MICROSERVICIO ---
            bool tieneCitas = await _reservacionClient.TieneReservasEnRango(
               estilistaId,
               DateOnly.FromDateTime(dto.FechaInicio),
               DateOnly.FromDateTime(dto.FechaFin)
           );

            if (tieneCitas)
            {
                throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS,
                    "No se puede mover el bloqueo a este rango porque existen reservaciones agendadas.");
            }
            // --------------------------------

            var bloqueo = new BloqueoRangoDiasLibres
            {
                Id = bloqueoId,
                EstilistaId = estilistaId,
                FechaInicioBloqueo = dto.FechaInicio.Date,
                FechaFinBloqueo = dto.FechaFin.Date,
                Razon = dto.Razon
            };

            var success = await _agendaRepo.UpdateBloqueoDiasLibresAsync(bloqueo);
            if (!success)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA, "Bloqueo no encontrado.");

            // Publicar evento...
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

        // --- 5. ELIMINAR BLOQUEO ---
        public async Task<bool> DeleteBloqueoDiasLibresAsync(int estilistaId, int bloqueoId)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            var bloqueosActuales = await _agendaRepo.GetBloqueosDiasLibresAsync(estilistaId);
            var bloqueoAEliminar = bloqueosActuales.FirstOrDefault(b => b.Id == bloqueoId);
            if (bloqueoAEliminar == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA, "El bloqueo no existe.");

            // AQUÍ NO HAY VALIDACIÓN DE CITAS:
            // Liberar un bloqueo (borrar vacaciones) significa que el estilista vuelve a trabajar.
            // Eso nunca genera conflicto con citas, al contrario, abre espacio.

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

        // Métodos de lectura (Getters) y DeleteDescansoFijo se mantienen igual...
        public async Task DeleteDescansoFijoAsync(int estilistaId, DayOfWeek dia)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(estilistaId);
            if (estilista == null) throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);
            await _agendaRepo.DeleteDescansoFijoAsync(estilistaId, dia);
            // Publicar evento...
            var eventoEliminacion = new { EstilistaId = estilistaId, DiaSemana = dia, Accion = "ELIMINADO" };
            await _messagePublisher.PublishAsync(eventoEliminacion, "descanso_fijo.eliminado", EXCHANGE_NAME);
        }

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
            return result.Select(b => new BloqueoResponseDto { Id = b.Id, FechaInicio = b.FechaInicioBloqueo, FechaFin = b.FechaFinBloqueo, Razon = b.Razon }).ToList();
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