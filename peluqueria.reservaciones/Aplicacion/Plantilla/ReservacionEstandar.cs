using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Excepciones;
using peluqueria.reservaciones.Core.Puertos.Salida;
using System.Threading.Tasks;
using System;
using System.Linq; 
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Aplicacion.Plantilla
{
    public class ReservacionEstandar : ReservacionPlantillaBase
    {
        private readonly IServicioRepositorio _servicioRepositorio;
        private readonly IEstilistaRepositorio _estilistaRepositorio;
        private readonly IHorarioRepositorio _horarioRepositorio;
        private readonly IReservacionRepositorio _reservacionRepositorio;

        // El constructor inyecta las dependencias necesarias para los pasos abstractos.
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

        //Validar los datos de la reservacion para evitar nulos
        public override Task<Reservacion> ValidarDatosAsync(Reservacion reservacion)
        {
            // Valida que los campos de Dominio esenciales para la reserva no sean valores por defecto/nulos
            if (reservacion.Fecha == default ||
                reservacion.HoraInicio == default ||
                reservacion.EstilistaId <= 0 ||
                reservacion.ServicioId <= 0 ||
                string.IsNullOrEmpty(reservacion.ClienteIdentificacion))
            {
                // Lanzamos un Exepcion de Dominio si falta algún dato esencial
                throw new ValidacionDominioExcepcion("Datos de la reservación incompletos. Verifique fecha, hora, IDs y cliente.");
            }


            return Task.FromResult(reservacion);
        }

        // Verificar que el servicio y estilista existan
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

        // Validar la disponibilidad del estilista para la fecha y hora solicitada
        public override async Task<Reservacion> ValidarDisponibilidadAsync(Reservacion reservacion)
        {
            var estilistaId = reservacion.EstilistaId;
            var fechaReserva = reservacion.Fecha;
            var horaInicioReserva = reservacion.HoraInicio;


            var horaFinReserva = reservacion.HoraInicio.AddMinutes(reservacion.TiempoAtencion);

            // Convertir DateOnly a DateTime para obtener el Día de la Semana
            var fechaHoraInicio = fechaReserva.ToDateTime(horaInicioReserva);
            DayOfWeek diaSemana = fechaHoraInicio.DayOfWeek;

            // Validar el rango de dias libres
            var bloqueoRango = await _horarioRepositorio.GetRangoDiasLibres(estilistaId);

            // Verificamos si la fecha de la reserva está DENTRO de un bloqueo de días completo.
            if (bloqueoRango != null &&
                fechaHoraInicio >= bloqueoRango.FechaInicioBloqueo &&
                fechaHoraInicio <= bloqueoRango.FechaFinBloqueo)
            {
                throw new ValidacionDisponibilidadExeption($"El estilista tiene un bloqueo de días completo del {bloqueoRango.FechaInicioBloqueo} al {bloqueoRango.FechaFinBloqueo}.");
            }

            // Validar horario
            var horarioBase = await _horarioRepositorio.GetStylistScheduleAsync(estilistaId);

            // En caso de que el estilista no tenga un horario base 
            if (horarioBase == null)
            {
                throw new ValidacionDisponibilidadExeption("El estilista no tiene un horario base semanal configurado.");
            }

            // Encontrar el horario para el día de la semana de la reserva
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

            // Validacion de los descansos fijos
            var descansoFijo = await _horarioRepositorio.GetDescanso(estilistaId);

            if (descansoFijo != null)
            {
                // Encontrar descansos para el día de la semana de la reserva
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

            // 4. Validacion de reservaciones existentes
            var horaFinCalculada = reservacion.HoraInicio.AddMinutes(reservacion.TiempoAtencion);

            var conflictos = await _reservacionRepositorio.ObtenerReservasConflictivasAsync(
                reservacion.EstilistaId,
                reservacion.Fecha,
                reservacion.HoraInicio,
                horaFinCalculada
            );

            if (conflictos.Any())
            {
                // Puedes ser más específico mostrando con cuál choca
                var conflicto = conflictos.First();
                throw new ValidacionDisponibilidadExeption(
                    $"El estilista ya tiene una cita reservada de {conflicto.HoraInicio} a {conflicto.HoraFin}.");
            }


            return reservacion;
        }

        // Calcular el tiempo de atencion total de la reservacion
        public override Task<Reservacion> CalcularTiempoAtencionAsync(Reservacion reservacion)
        {
            var duracionServicio = reservacion.Servicio.DuracionMinutos;
            var tiempoPreparacion = 15;
            reservacion.TiempoAtencion = duracionServicio+tiempoPreparacion; // 60 minutos

            return Task.FromResult(reservacion);
        }

        //Guardar la reservacion en el repositorio
        public override async Task<Reservacion> PersistirReservacionAsync(Reservacion reservacion)
        {
            reservacion.Estado = "PENDIENTE"; // Establecer el estado inicial de la reservación
            var reservaGuardada = await _reservacionRepositorio.GuardarAsync(reservacion);
            return reservaGuardada;
        }
    }
}