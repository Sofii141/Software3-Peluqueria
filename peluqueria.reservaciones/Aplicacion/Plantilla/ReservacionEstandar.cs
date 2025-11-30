using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Excepciones;
using peluqueria.reservaciones.Core.Puertos.Salida;
using System.Threading.Tasks;
using System;
using System.Linq; 
using System.Threading.Tasks;

/*
 @author: ChatGPT
 @description: Implementación concreta del patrón de plantilla para la creacion de una reservación estandar.
 */

namespace peluqueria.reservaciones.Aplicacion.Plantilla
{
    public class ReservacionEstandar : ReservacionPlantillaBase
    {
        private readonly IServicioRepositorio _servicioRepositorio;
        private readonly IEstilistaRepositorio _estilistaRepositorio;
        private readonly IHorarioRepositorio _horarioRepositorio;
        private readonly IReservacionRepositorio _reservacionRepositorio;

        public ReservacionEstandar(
            IServicioRepositorio servicioRepositorio,
            IEstilistaRepositorio estilistaRepositorio,
            IHorarioRepositorio horarioRepositorio,
            IReservacionRepositorio reservacionRepositorio)
        {
            _servicioRepositorio = servicioRepositorio;
            _estilistaRepositorio = estilistaRepositorio;
            _horarioRepositorio = horarioRepositorio;
            _reservacionRepositorio = reservacionRepositorio;
        }

        public override Task<Reservacion> ValidarDatosAsync(Reservacion reservacion)
        {
            if (reservacion.Fecha == default ||
                reservacion.HoraInicio == default ||
                reservacion.EstilistaId <= 0 ||
                reservacion.ServicioId <= 0 ||
                string.IsNullOrEmpty(reservacion.ClienteIdentificacion))
            {
                throw new ValidacionDominioExcepcion("Datos de la reservación incompletos. Verifique fecha, hora, IDs y cliente.");
            }


            return Task.FromResult(reservacion);
        }

        public override async Task<Reservacion> ValidarServicioEstilistaAsync(Reservacion reservacion)
        {
            var servicio = await _servicioRepositorio.GetByIdAsync(reservacion.ServicioId);

            if (servicio == null)
            {
                throw new ValidacionDatosExeption($"El servicio con ID {reservacion.ServicioId} no existe.");
            }

            if (!servicio.Disponible)
            {
                throw new ValidacionDatosExeption($"El servicio '{servicio.Nombre}' no está activo actualmente.");
            }

            var estilista = await _estilistaRepositorio.GetByIdAsync(reservacion.EstilistaId);

            if (estilista == null)
            {
                throw new ValidacionDatosExeption($"El estilista con ID {reservacion.EstilistaId} no existe.");
            }

            if (!estilista.EstaActivo)
            {
                throw new ValidacionDatosExeption($"El estilista '{estilista.NombreCompleto}' no está activo.");
            }

            reservacion.Servicio = servicio;
            reservacion.Estilista = estilista;

            return reservacion;
        }

        public override async Task<Reservacion> ValidarDisponibilidadAsync(Reservacion reservacion)
        {
            var estilistaId = reservacion.EstilistaId;
            var fechaReserva = reservacion.Fecha;
            var horaInicioReserva = reservacion.HoraInicio;


            var horaFinReserva = reservacion.HoraInicio.AddMinutes(reservacion.TiempoAtencion);

            var fechaHoraInicio = fechaReserva.ToDateTime(horaInicioReserva);
            DayOfWeek diaSemana = fechaHoraInicio.DayOfWeek;

            var bloqueoRango = await _horarioRepositorio.GetRangoDiasLibres(estilistaId);

            if (bloqueoRango != null &&
                fechaHoraInicio >= bloqueoRango.FechaInicioBloqueo &&
                fechaHoraInicio <= bloqueoRango.FechaFinBloqueo)
            {
                throw new ValidacionDisponibilidadExeption($"El estilista tiene un bloqueo de días completo del {bloqueoRango.FechaInicioBloqueo} al {bloqueoRango.FechaFinBloqueo}.");
            }

            var horarioBase = await _horarioRepositorio.GetStylistScheduleAsync(estilistaId);

            if (horarioBase == null)
            {
                throw new ValidacionDisponibilidadExeption("El estilista no tiene un horario base semanal configurado.");
            }

            var horarioDia = horarioBase.HorariosSemanales
                .FirstOrDefault(h => h.DiaSemana == diaSemana);

            if (horarioDia == null || !horarioDia.EsLaborable)
            {
                throw new ValidacionDisponibilidadExeption($"El estilista no trabaja el día {diaSemana} (Horario Base).");
            }

            var horaFinHorario = TimeOnly.FromTimeSpan(horarioDia.HoraFin);
            var horaInicioHorario = TimeOnly.FromTimeSpan(horarioDia.HoraInicio);
            if (horaInicioReserva < horaInicioHorario || horaFinReserva > horaFinHorario)
            {
                throw new ValidacionDisponibilidadExeption($"La reservación ({horaInicioReserva} a {horaFinReserva}) está fuera del horario laboral del estilista ({horarioDia.HoraInicio} a {horarioDia.HoraFin}) para el día {diaSemana}.");
            }

            var descansoFijo = await _horarioRepositorio.GetDescanso(estilistaId);

            if (descansoFijo != null)
            {
                var descansosDia = descansoFijo.DescansosFijos
                    .Where(d => d.DiaSemana == diaSemana);

                foreach (var descanso in descansosDia)
                {
                    var horaFinDescanso = TimeOnly.FromTimeSpan(descanso.HoraFin);
                    var horaInicioDescanso = TimeOnly.FromTimeSpan(descanso.HoraInicio);
                    if (horaInicioReserva < horaFinDescanso && horaFinReserva > horaInicioDescanso)
                    {
                        throw new ValidacionDisponibilidadExeption($"La reservación se superpone con un descanso fijo del estilista ({descanso.HoraInicio} a {descanso.HoraFin}) el día {diaSemana}.");
                    }
                }
            }

            var horaFinCalculada = reservacion.HoraInicio.AddMinutes(reservacion.TiempoAtencion);

            var conflictos = await _reservacionRepositorio.ObtenerReservasConflictivasAsync(
                reservacion.EstilistaId,
                reservacion.Fecha,
                reservacion.HoraInicio,
                horaFinCalculada
            );

            if (conflictos.Any())
            {
                var conflicto = conflictos.First();
                throw new ValidacionDisponibilidadExeption(
                    $"El estilista ya tiene una cita reservada de {conflicto.HoraInicio} a {conflicto.HoraFin}.");
            }


            return reservacion;
        }

        public override Task<Reservacion> CalcularTiempoAtencionAsync(Reservacion reservacion)
        {
            var duracionServicio = reservacion.Servicio.DuracionMinutos;
            var tiempoPreparacion = 15;
            reservacion.TiempoAtencion = duracionServicio+tiempoPreparacion; // 60 minutos
            reservacion.HoraFin = reservacion.HoraInicio.AddMinutes(reservacion.TiempoAtencion);

            return Task.FromResult(reservacion);
        }

        public override async Task<Reservacion> PersistirReservacionAsync(Reservacion reservacion)
        {
            reservacion.Estado = "PENDIENTE"; 
            var reservaGuardada = await _reservacionRepositorio.GuardarAsync(reservacion);
            return reservaGuardada;
        }
    }
}