using peluqueria.reservaciones.Aplicacion.Comandos;
using peluqueria.reservaciones.Aplicacion.Mapeo; 
using peluqueria.reservaciones.Aplicacion.Plantilla;
using peluqueria.reservaciones.Core.Puertos.Entrada;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Excepciones;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.DTO.Comunicacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Aplicacion.Fachada
{
    public class ReservacionManejador : IReservacionManejador
    {
        private readonly ReservacionPlantillaBase _creacionPlantilla;
        private readonly IReservacionRepositorio _reservacionRepositorio;
        private readonly IHorarioRepositorio _horarioRepositorio;

        public ReservacionManejador(
            ReservacionPlantillaBase creacionPlantilla,
            IReservacionRepositorio reservacionRepositorio,
            IHorarioRepositorio horarioRepositorio)
        {
            _creacionPlantilla = creacionPlantilla; // Instancia de ReservacionEstandar
            _reservacionRepositorio = reservacionRepositorio;
            _horarioRepositorio = horarioRepositorio;
        }

        // Crear una nueva reservación (Usa la Plantilla de Creación)
        public async Task<ReservacionRespuestaDTO> CrearReservacionAsync(CrearReservacionComando comando)
        {
            // Mapeo: Comando -> Entidad
            var reservacion = new Reservacion
            {
                Fecha = comando.Fecha,
                HoraInicio = comando.HoraInicio,
                ClienteIdentificacion = comando.ClienteIdentificacion,
                ServicioId = comando.ServicioId,
                EstilistaId = comando.EstilistaId,
                Estado = "INICIADA",
                TiempoAtencion = 0 // Será calculado
            };

           
            Reservacion reservacionCreada = await _creacionPlantilla.ProcesarReservacionAsync(reservacion);

            // Entidad -> DTO de Respuesta
            return ReservacionMapper.ToRespuestaDTO(reservacionCreada);
        }

        // Reprogramar reservacion
        public async Task<ReservacionRespuestaDTO> ReprogramarReservacionAsync(
            int reservacionId, ReservacionPeticionDTO peticion) 
        {
            // 1. Obtener la reservación existente
            var reservacion = await _reservacionRepositorio.ObtenerPorIdAsync(reservacionId);
            if (reservacion == null)
            {
                throw new ValidacionDatosExeption($"Reservación ID {reservacionId} no encontrada.");
            }

            var nuevaFecha = peticion.Fecha;
            var nuevaHoraInicio = peticion.HoraInicio;

            // 2. Establecer la nueva hora final (asume que TiempoAtencion no cambia)
            var nuevaHoraFin = nuevaHoraInicio.AddMinutes(reservacion.TiempoAtencion);

            // 3. VALIDACIÓN DE DISPONIBILIDAD para el nuevo slot (la lógica restante es la misma)
            var conflictos = await _reservacionRepositorio.ObtenerReservasConflictivasAsync(
                reservacion.EstilistaId,
                nuevaFecha,
                nuevaHoraInicio,
                nuevaHoraFin
            );

            var conflictosReales = conflictos.Where(r => r.Id != reservacionId).ToList();

            if (conflictosReales.Any())
            {
                throw new ValidacionDisponibilidadExeption("El nuevo horario se superpone con otra reserva existente.");
            }

            // 4. Actualizar la entidad
            reservacion.Fecha = nuevaFecha;
            reservacion.HoraInicio = nuevaHoraInicio;
            reservacion.HoraFin = nuevaHoraFin;

            // 5. Persistir el cambio
            await _reservacionRepositorio.ActualizarAsync(reservacion);

            // 6. Mapeo: Entidad -> DTO de Respuesta
            return ReservacionMapper.ToRespuestaDTO(reservacion);
        }

        // cancelar reservacion
        public async Task CancelarReservacionAsync(int reservacionId)
        {
            await _reservacionRepositorio.CancelarAsync(reservacionId);
        }

        // consultar reservaciones de un cliente
        public async Task<List<ReservacionRespuestaDTO>> ConsultarReservacionesClienteAsync(string clienteIdentificacion)
        {
            var listaReservaciones = await _reservacionRepositorio.BuscarReservasPorClienteAsync(clienteIdentificacion);

            // 2. Mapear la lista de Entidades a la lista de DTOs de Respuesta
            return listaReservaciones.Select(ReservacionMapper.ToRespuestaDTO).ToList();
        }
    }
}