using Microsoft.AspNetCore.Mvc;
using peluqueria.reservaciones.Aplicacion.DTO.Comunicacion;
using peluqueria.reservaciones.Aplicacion.Puertos.Entrada;
using System.Net;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Api.Controladores
{
    [ApiController]
    [Route("api/reportes")] 
    public class ReportesController : ControllerBase
    {
        private readonly IReservacionManejador _manejador;

        public ReportesController(IReservacionManejador manejador)
        {
            _manejador = manejador;
        }


        [HttpPost("estilista")]
        [ProducesResponseType(typeof(InfoReportesRespuestaDTO), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ObtenerReporteEstilista([FromBody] PeticionReservasEstilistaDTO peticion)
        {
            var reporte = await _manejador.GenerarReporteEstilistaAsync(peticion);
            return Ok(reporte);
        }
    }
}