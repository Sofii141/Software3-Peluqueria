using Microsoft.AspNetCore.Mvc;
using peluqueria.reservaciones.Aplicacion.Comandos;
using peluqueria.reservaciones.Core.Puertos.Entrada;
using peluqueria.reservaciones.Core.Excepciones;
using peluqueria.reservaciones.Infraestructura.DTO.Comunicacion;
using System.Net;
using System.Threading.Tasks;

/*
 @autor: Juan David Moran
 @descripcion: Controlador API para gestionar reservaciones en la peluquería.
 */

namespace peluqueria.reservaciones.Infraestructura.Controladores
{
    [ApiController]
    [Route("api/reservaciones")]
    public class ReservacionesController : ControllerBase
    {
        private readonly IReservacionManejador _manejador;

        public ReservacionesController(IReservacionManejador manejador)
        {
            _manejador = manejador;
        }

        // Crear Reservacion
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CrearReservacion([FromBody] ReservacionPeticionDTO peticion)
        {
            try
            {
                var comando = new CrearReservacionComando
                {
                    Fecha = peticion.Fecha,
                    HoraInicio = peticion.HoraInicio,
                    EstilistaId = peticion.EstilistaId,
                    ServicioId = peticion.ServicioId,
                    ClienteIdentificacion = peticion.ClienteIdentificacion
                };

                var respuesta = await _manejador.CrearReservacionAsync(comando);

                return StatusCode((int)HttpStatusCode.Created, respuesta);
            }
            catch (ValidacionDominioExcepcion ex)
            {
                return BadRequest(new { error = "Validacion", mensaje = ex.Message , codigoError = "G-ERROR-017"});
            }
            catch (ValidacionDisponibilidadExeption ex)
            {
                return BadRequest(new { error = "Disponibilidad", mensaje = ex.Message , codigoError = "G-ERROR-018" });
            }
            catch (ValidacionDatosExeption ex)
            {
                return BadRequest(new { error = "DatosEntrada", mensaje = ex.Message , codigoError = "G-ERROR-019" });
            }
        }

        //ReprogramarDTO
        [HttpPut("{reservacionId}/reprogramar")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ReprogramarReservacion(
    int reservacionId,
    [FromBody] ReservacionPeticionDTO peticion) 
        {
            try
            {
                
                var respuesta = await _manejador.ReprogramarReservacionAsync(
                    reservacionId,
                    peticion);

                return Ok(respuesta);
            }
           
            catch (ValidacionDatosExeption ex)
            {
                return NotFound(new { error = "NoEncontrado", mensaje = ex.Message , codigoError = "G-ERROR-020" });
            }
            catch (ValidacionDisponibilidadExeption ex)
            {
                return BadRequest(new { error = "Disponibilidad", mensaje = ex.Message , codigoError = "G-ERROR-018" });
            }
        }

        //CancelarReservacion
        [HttpDelete("{reservacionId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)] 
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CancelarReservacion(int reservacionId)
        {
            try
            {
                await _manejador.CancelarReservacionAsync(reservacionId);
                return NoContent();
            }
            catch (ValidacionDatosExeption ex)
            {
                return NotFound(new { error = "NoEncontrado", mensaje = ex.Message });
            }
        }

        //Consultar reservaciones por cliente
        [HttpGet("{clienteIdentificacion}")]
        [ProducesResponseType((int)HttpStatusCode.OK)] 
        public async Task<IActionResult> ConsultarReservacionesCliente(string clienteIdentificacion)
        {
            var listaReservaciones = await _manejador.ConsultarReservacionesClienteAsync(clienteIdentificacion);

            if (listaReservaciones == null || listaReservaciones.Count == 0)
            {
                return NotFound($"No se encontraron reservaciones para el cliente {clienteIdentificacion}.");
            }

            return Ok(listaReservaciones);
        }

        // cambiar estado de una reservacion
        [HttpPatch("cambiar-estado")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CambiarEstado([FromBody] CambioEstadoDTO peticion)
        {
            try
            {
                await _manejador.CambiarEstadoReservacionAsync(peticion);
                return NoContent();
            }
            catch (ValidacionDatosExeption ex)
            {
                return NotFound(new { error = "NoEncontrado", mensaje = ex.Message });
            }
        }

        // Buscar reservas de estilista por rango de fechas
        [HttpPost("buscar/estilista-rango")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> BuscarPorRango([FromBody] PeticionReservasEstilistaDTO peticion)
        {
            var lista = await _manejador.ConsultarReservasEstilistaRangoAsync(peticion);

            return Ok(lista);
        }

        // Buscar reservas de estilista por fecha específica
        [HttpPost("buscar/estilista-fecha")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> BuscarPorFecha([FromBody] PeticionReservaEstilistaFechaDTO peticion)
        {
            var lista = await _manejador.ConsultarReservasEstilistaFechaAsync(peticion);

            return Ok(lista);
        }

        // Obtener todas las reservaciones
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ObtenerTodas()
        {
            var lista = await _manejador.ConsultarTodasLasReservacionesAsync();
            return Ok(lista);
        }
    }
}