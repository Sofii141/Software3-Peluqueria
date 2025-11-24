using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.API.Controllers
{
    [Route("api/estilistas/agenda")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EstilistaAgendaController : ControllerBase
    {
        private readonly IEstilistaAgendaService _agendaService;

        public EstilistaAgendaController(IEstilistaAgendaService agendaService)
        {
            _agendaService = agendaService;
        }

        [HttpPut("{estilistaId:int}/horario-base")]
        public async Task<IActionResult> UpdateHorarioBase(int estilistaId, [FromBody] List<HorarioDiaDto> horarios)
        {
            if (!ModelState.IsValid || horarios == null || horarios.Count == 0) return BadRequest("Datos inválidos.");

            try
            {
                await _agendaService.UpdateHorarioBaseAsync(estilistaId, horarios);
                return Ok(new { message = "Horario base actualizado correctamente." });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("{estilistaId:int}/horario-base")]
        public async Task<IActionResult> GetHorarioBase(int estilistaId)
        {
            var horarios = await _agendaService.GetHorarioBaseAsync(estilistaId);
            return Ok(horarios);
        }

        [HttpPut("{estilistaId:int}/descanso-fijo")]
        public async Task<IActionResult> UpdateDescansoFijo(int estilistaId, [FromBody] List<HorarioDiaDto> descansos)
        {
            try
            {
                await _agendaService.UpdateDescansoFijoAsync(estilistaId, descansos);
                return Ok(new { message = "Descansos actualizados (se ignoraron días no laborables)." });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("{estilistaId:int}/descanso-fijo")]
        public async Task<IActionResult> GetDescansosFijos(int estilistaId)
        {
            var descansos = await _agendaService.GetDescansosFijosAsync(estilistaId);
            return Ok(descansos);
        }

        [HttpDelete("{estilistaId:int}/descanso-fijo/{dia}")]
        public async Task<IActionResult> DeleteDescansoFijo(int estilistaId, DayOfWeek dia)
        {
            await _agendaService.DeleteDescansoFijoAsync(estilistaId, dia);
            return NoContent();
        }

        [HttpPost("{estilistaId:int}/bloqueo-dias-libres")]
        public async Task<IActionResult> CreateBloqueoDiasLibres(int estilistaId, [FromBody] BloqueoRangoDto bloqueoDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _agendaService.CreateBloqueoDiasLibresAsync(estilistaId, bloqueoDto);
                if (!success) return NotFound("Estilista no encontrado.");

                return Created("", new { message = "Bloqueo creado correctamente." });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{estilistaId:int}/bloqueo-dias-libres/{bloqueoId:int}")]
        public async Task<IActionResult> UpdateBloqueo(int estilistaId, int bloqueoId, [FromBody] BloqueoRangoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _agendaService.UpdateBloqueoDiasLibresAsync(estilistaId, bloqueoId, dto);
            if (!success) return NotFound("Bloqueo no encontrado o no pertenece al estilista.");

            return Ok(new { message = "Bloqueo actualizado correctamente." });
        }

        [HttpDelete("{estilistaId:int}/bloqueo-dias-libres/{bloqueoId:int}")]
        public async Task<IActionResult> DeleteBloqueo(int estilistaId, int bloqueoId)
        {
            var success = await _agendaService.DeleteBloqueoDiasLibresAsync(estilistaId, bloqueoId);
            if (!success) return NotFound("Bloqueo no encontrado.");

            return NoContent();
        }

        [HttpGet("{estilistaId:int}/bloqueo-dias-libres")]
        public async Task<IActionResult> GetBloqueosDiasLibres(int estilistaId)
        {
            var bloqueos = await _agendaService.GetBloqueosDiasLibresAsync(estilistaId);
            return Ok(bloqueos);
        }
    }
}