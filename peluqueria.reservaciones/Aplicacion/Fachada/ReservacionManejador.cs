using peluqueria.reservaciones.Aplicacion.Comandos;
using peluqueria.reservaciones.Aplicacion.Mapeo; 
using peluqueria.reservaciones.Aplicacion.Plantilla;
using peluqueria.reservaciones.Core.Puertos.Entrada;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Excepciones;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Aplicacion.DTO.Comunicacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*
 @autor: ChatGPT
 @descripcion: Implementación del intermediario para las operaciones de reservaciones que utiliza el patrón de plantilla para crear y reprogramar reservaciones. 
 */

namespace peluqueria.reservaciones.Aplicacion.Fachada
{
    public class ReservacionManejador : IReservacionManejador
    {
        private readonly ReservacionPlantillaBase _creacionPlantilla;
        private readonly ReservacionReprogramar _reprogramacionPlantilla;   
        private readonly IReservacionRepositorio _reservacionRepositorio;
        private readonly IHorarioRepositorio _horarioRepositorio;

        public ReservacionManejador(
            ReservacionEstandar creacionPlantilla,
            ReservacionReprogramar reprogramacionPlantilla,
            IReservacionRepositorio reservacionRepositorio,
            IHorarioRepositorio horarioRepositorio)
        {
            _creacionPlantilla = creacionPlantilla; 
            _reprogramacionPlantilla = reprogramacionPlantilla;
            _reservacionRepositorio = reservacionRepositorio;
            _horarioRepositorio = horarioRepositorio;
        }

        // Crear una nueva reservación (Usa la Plantilla de Creación)
        public async Task<ReservacionRespuestaDTO> CrearReservacionAsync(CrearReservacionComando comando)
        {
            var reservacion = new Reservacion
            {
                Fecha = comando.Fecha,
                HoraInicio = comando.HoraInicio,
                ClienteIdentificacion = comando.ClienteIdentificacion,
                ServicioId = comando.ServicioId,
                EstilistaId = comando.EstilistaId,
                Estado = "INICIADA",
                TiempoAtencion = 0 
            };

           
            Reservacion reservacionCreada = await _creacionPlantilla.ProcesarReservacionAsync(reservacion);

            return ReservacionMapper.ToRespuestaDTO(reservacionCreada);
        }

        // Reprogramar reservacion (Usa la plantilla de reprogramar)
        public async Task<ReservacionRespuestaDTO> ReprogramarReservacionAsync(
            int reservacionId, ReservacionPeticionDTO peticion)
        {
            var reservacion = await _reservacionRepositorio.ObtenerPorIdAsync(reservacionId);
            if (reservacion == null)
            {
                throw new ValidacionDatosExeption($"Reservación ID {reservacionId} no encontrada.");
            }

            reservacion.Fecha = peticion.Fecha;
            reservacion.HoraInicio = peticion.HoraInicio;


            Reservacion reservacionActualizada = await _reprogramacionPlantilla.ProcesarReservacionAsync(reservacion);

            return ReservacionMapper.ToRespuestaDTO(reservacionActualizada);
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

            return listaReservaciones.Select(ReservacionMapper.ToRespuestaDTO).ToList();
        }

        // cambiar estado de una reservacion
        public async Task CambiarEstadoReservacionAsync(CambioEstadoDTO peticion)
        {
            var reservacion = await _reservacionRepositorio.ObtenerPorIdAsync(peticion.ReservacionId);
            if (reservacion == null)
            {
                throw new ValidacionDatosExeption($"Reservación ID {peticion.ReservacionId} no encontrada.");
            }

            await _reservacionRepositorio.CambiarEstadoAsync(peticion.ReservacionId, peticion.NuevoEstado);
        }

        // consultar reservas de un estilista en un rango de fechas
        public async Task<List<ReservacionRespuestaDTO>> ConsultarReservasEstilistaRangoAsync(PeticionReservasEstilistaDTO peticion)
        {
            var lista = await _reservacionRepositorio.BuscarReservasEstilistaRangoAsync(
                peticion.EstilistaId,
                peticion.FechaInicio,
                peticion.FechaFin);

            return lista.Select(ReservacionMapper.ToRespuestaDTO).ToList();
        }

        // consultar reservas de un estilista en una fecha específica
        public async Task<List<ReservacionRespuestaDTO>> ConsultarReservasEstilistaFechaAsync(PeticionReservaEstilistaFechaDTO peticion)
        {
            var lista = await _reservacionRepositorio.BuscarReservasPorEstilistaAsync(
                peticion.EstilistaId,
                peticion.Fecha);

            return lista.Select(ReservacionMapper.ToRespuestaDTO).ToList();
        }

        // consultar todas las reservaciones
        public async Task<List<ReservacionRespuestaDTO>> ConsultarTodasLasReservacionesAsync()
        {
            var listaReservaciones = await _reservacionRepositorio.ObtenerTodasAsync();

            return listaReservaciones.Select(ReservacionMapper.ToRespuestaDTO).ToList();
        }

    }
}