using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.API.Controllers
{
    /// <summary>
    /// Controlador encargado de gestionar la configuración de horarios y disponibilidad de los estilistas.
    /// Maneja el horario base semanal, los descansos fijos (almuerzos) y los bloqueos por días libres o vacaciones.
    /// </summary>
    /// <remarks>
    /// Todos los endpoints de este controlador requieren permisos de Administrador.
    /// </remarks>
    [Route("api/estilistas/agenda")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EstilistaAgendaController : ControllerBase
    {
        private readonly IEstilistaAgendaService _agendaService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EstilistaAgendaController"/>.
        /// </summary>
        /// <param name="agendaService">Servicio de aplicación para la gestión de la agenda.</param>
        public EstilistaAgendaController(IEstilistaAgendaService agendaService)
        {
            _agendaService = agendaService;
        }

        /// <summary>
        /// Actualiza el horario base semanal (jornada laboral) de un estilista.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <param name="horarios">Lista de configuración diaria (Día, Hora Inicio, Hora Fin, EsLaborable).</param>
        /// <returns>Confirmación de la actualización.</returns>
        /// <response code="200">Si el horario se actualizó correctamente.</response>
        /// <response code="400">Si hay conflicto con citas existentes al reducir el horario o marcar día no laborable.</response>
        /// <response code="404">Si el estilista no existe.</response>
        [HttpPut("{estilistaId:int}/horario-base")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHorarioBase(int estilistaId, [FromBody] List<HorarioDiaDto> horarios)
        {
            await _agendaService.UpdateHorarioBaseAsync(estilistaId, horarios);

            return Ok(new { message = "Horario base actualizado correctamente." });
        }

        /// <summary>
        /// Obtiene la configuración del horario base semanal de un estilista.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <returns>Colección de horarios por día de la semana.</returns>
        /// <response code="200">Retorna la lista de horarios.</response>
        /// <response code="404">Si el estilista no existe.</response>
        [HttpGet("{estilistaId:int}/horario-base")]
        [ProducesResponseType(typeof(IEnumerable<HorarioDiaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHorarioBase(int estilistaId)
        {
            var horarios = await _agendaService.GetHorarioBaseAsync(estilistaId);
            return Ok(horarios);
        }

        /// <summary>
        /// Actualiza los descansos fijos recurrentes (ej. hora de almuerzo) para un estilista.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <param name="descansos">Lista de descansos a configurar.</param>
        /// <returns>Confirmación de la actualización.</returns>
        /// <response code="200">Si los descansos se actualizaron correctamente.</response>
        /// <response code="400">Si el descanso choca con citas existentes o se asigna en un día no laborable.</response>
        [HttpPut("{estilistaId:int}/descanso-fijo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDescansoFijo(int estilistaId, [FromBody] List<HorarioDiaDto> descansos)
        {
            await _agendaService.UpdateDescansoFijoAsync(estilistaId, descansos);
            return Ok(new { message = "Descansos actualizados." });
        }

        /// <summary>
        /// Obtiene la lista de descansos fijos configurados para un estilista.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <returns>Colección de descansos configurados.</returns>
        [HttpGet("{estilistaId:int}/descanso-fijo")]
        [ProducesResponseType(typeof(IEnumerable<HorarioDiaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDescansosFijos(int estilistaId)
        {
            var descansos = await _agendaService.GetDescansosFijosAsync(estilistaId);
            return Ok(descansos);
        }

        /// <summary>
        /// Elimina un descanso fijo para un día específico de la semana.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <param name="dia">Día de la semana del descanso a eliminar.</param>
        /// <returns>Sin contenido.</returns>
        /// <response code="204">Si el descanso fue eliminado.</response>
        [HttpDelete("{estilistaId:int}/descanso-fijo/{dia}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteDescansoFijo(int estilistaId, DayOfWeek dia)
        {
            await _agendaService.DeleteDescansoFijoAsync(estilistaId, dia);
            return NoContent();
        }

        /// <summary>
        /// Crea un bloqueo de días libres (ej. vacaciones o permisos) en un rango de fechas.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <param name="bloqueoDto">Datos del rango de fechas y razón del bloqueo.</param>
        /// <returns>Mensaje de creación exitosa.</returns>
        /// <response code="201">Si el bloqueo fue creado.</response>
        /// <response code="400">Si las fechas son inválidas o existen citas en ese rango.</response>
        [HttpPost("{estilistaId:int}/bloqueo-dias-libres")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBloqueoDiasLibres(int estilistaId, [FromBody] BloqueoRangoDto bloqueoDto)
        {
            await _agendaService.CreateBloqueoDiasLibresAsync(estilistaId, bloqueoDto);

            return Created("", new { message = "Bloqueo creado correctamente." });
        }

        /// <summary>
        /// Actualiza un bloqueo de días libres existente.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <param name="bloqueoId">Identificador del bloqueo a modificar.</param>
        /// <param name="dto">Nuevos datos del rango de fechas.</param>
        /// <returns>Mensaje de confirmación.</returns>
        /// <response code="200">Si el bloqueo fue actualizado.</response>
        /// <response code="400">Si existe conflicto con citas o fechas inválidas.</response>
        [HttpPut("{estilistaId:int}/bloqueo-dias-libres/{bloqueoId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBloqueo(int estilistaId, int bloqueoId, [FromBody] BloqueoRangoDto dto)
        {
            await _agendaService.UpdateBloqueoDiasLibresAsync(estilistaId, bloqueoId, dto);
            return Ok(new { message = "Bloqueo actualizado correctamente." });
        }

        /// <summary>
        /// Elimina un bloqueo de días libres (vacaciones).
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <param name="bloqueoId">Identificador del bloqueo a eliminar.</param>
        /// <returns>Sin contenido.</returns>
        /// <response code="204">Si el bloqueo fue eliminado correctamente.</response>
        [HttpDelete("{estilistaId:int}/bloqueo-dias-libres/{bloqueoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteBloqueo(int estilistaId, int bloqueoId)
        {
            await _agendaService.DeleteBloqueoDiasLibresAsync(estilistaId, bloqueoId);
            return NoContent();
        }

        /// <summary>
        /// Obtiene el historial de bloqueos de días libres (vacaciones) de un estilista.
        /// </summary>
        /// <param name="estilistaId">Identificador del estilista.</param>
        /// <returns>Lista de bloqueos registrados.</returns>
        [HttpGet("{estilistaId:int}/bloqueo-dias-libres")]
        [ProducesResponseType(typeof(IEnumerable<BloqueoResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBloqueosDiasLibres(int estilistaId)
        {
            var bloqueos = await _agendaService.GetBloqueosDiasLibresAsync(estilistaId);
            return Ok(bloqueos);
        }
    }
}