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
            await _agendaService.UpdateHorarioBaseAsync(estilistaId, horarios);

            return Ok(new { message = "Horario base actualizado correctamente." });
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
            await _agendaService.UpdateDescansoFijoAsync(estilistaId, descansos);
            return Ok(new { message = "Descansos actualizados." });
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
            await _agendaService.CreateBloqueoDiasLibresAsync(estilistaId, bloqueoDto);

            return Created("", new { message = "Bloqueo creado correctamente." });
        }

        [HttpPut("{estilistaId:int}/bloqueo-dias-libres/{bloqueoId:int}")]
        public async Task<IActionResult> UpdateBloqueo(int estilistaId, int bloqueoId, [FromBody] BloqueoRangoDto dto)
        {
            await _agendaService.UpdateBloqueoDiasLibresAsync(estilistaId, bloqueoId, dto);
            return Ok(new { message = "Bloqueo actualizado correctamente." });
        }

        [HttpDelete("{estilistaId:int}/bloqueo-dias-libres/{bloqueoId:int}")]
        public async Task<IActionResult> DeleteBloqueo(int estilistaId, int bloqueoId)
        {
            await _agendaService.DeleteBloqueoDiasLibresAsync(estilistaId, bloqueoId);
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