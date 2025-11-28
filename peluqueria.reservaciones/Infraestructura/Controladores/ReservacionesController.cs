using Microsoft.AspNetCore.Mvc;
using peluqueria.reservaciones.Aplicacion.Comandos;
using peluqueria.reservaciones.Core.Puertos.Entrada;
using peluqueria.reservaciones.Core.Excepciones;
using peluqueria.reservaciones.Infraestructura.DTO.Comunicacion;
using System.Net;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Infraestructura.Controladores
{
    // Controlador API para gestionar reservaciones
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
        [ProducesResponseType((int)HttpStatusCode.Created)] // 201
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // 400
        public async Task<IActionResult> CrearReservacion([FromBody] ReservacionPeticionDTO peticion)
        {
            try
            {
                // Mapeo DTO de Petición -> Comando (preparación para la capa de aplicación)
                var comando = new CrearReservacionComando
                {
                    Fecha = peticion.Fecha,
                    HoraInicio = peticion.HoraInicio,
                    EstilistaId = peticion.EstilistaId,
                    ServicioId = peticion.ServicioId,
                    ClienteIdentificacion = peticion.ClienteIdentificacion
                };

                // Llama al manejador para ejecutar la lógica de creación (Template Method)
                var respuesta = await _manejador.CrearReservacionAsync(comando);

                // Devuelve 201 Created
                return StatusCode((int)HttpStatusCode.Created, respuesta);
            }
            // Captura excepciones de negocio y las traduce a 400 Bad Request
            catch (ValidacionDominioExcepcion ex)
            {
                return BadRequest(new { error = "Validacion", mensaje = ex.Message });
            }
            catch (ValidacionDisponibilidadExeption ex)
            {
                return BadRequest(new { error = "Disponibilidad", mensaje = ex.Message });
            }
            catch (ValidacionDatosExeption ex)
            {
                return BadRequest(new { error = "DatosEntrada", mensaje = ex.Message });
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
                return NotFound(new { error = "NoEncontrado", mensaje = ex.Message });
            }
            catch (ValidacionDisponibilidadExeption ex)
            {
                return BadRequest(new { error = "Disponibilidad", mensaje = ex.Message });
            }
        }

        //CancelarReservacion
        [HttpDelete("{reservacionId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)] // 204
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CancelarReservacion(int reservacionId)
        {
            try
            {
                await _manejador.CancelarReservacionAsync(reservacionId);
                return NoContent();
            }
            //En caso de que el ID no exista
            catch (ValidacionDatosExeption ex)
            {
                return NotFound(new { error = "NoEncontrado", mensaje = ex.Message });
            }
        }

        //Consultar reservaciones por cliente
        [HttpGet("{clienteIdentificacion}")]
        [ProducesResponseType((int)HttpStatusCode.OK)] // 200
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

            // Retorna lista vacía [] si no hay datos, código 200 OK
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
    }
}