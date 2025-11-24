using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Estilista;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            if (!ModelState.IsValid || horarios == null || horarios.Count == 0)
            {
                return BadRequest("Debe proporcionar una lista válida de horarios.");
            }

            try
            {
                var success = await _agendaService.UpdateHorarioBaseAsync(estilistaId, horarios);

                if (!success) return NotFound("Estilista no encontrado.");

                // INFO-HU12-01: Horario semanal base actualizado correctamente.
                return Ok(new { message = "Horario semanal base actualizado correctamente." });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("RNI-H001"))
            {
                return BadRequest(ex.Message); // ERROR-HU12-01: Hora de inicio debe ser anterior a la hora de fin.
            }
        }

        // PEL-HU-13: Bloqueo de Días Libres (Vacaciones/Rango)
        [HttpPost("{estilistaId:int}/bloqueo-dias-libres")]
        public async Task<IActionResult> CreateBloqueoDiasLibres(int estilistaId, [FromBody] BloqueoRangoDto bloqueoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Antes de llamar al servicio, se haría la validación del bloqueo de citas futuras (RNI-H003 / G-ERROR-009)

            try
            {
                var success = await _agendaService.CreateBloqueoDiasLibresAsync(estilistaId, bloqueoDto);

                if (!success) return NotFound("Estilista no encontrado.");

                // INFO-HU13-01: Bloqueo de disponibilidad aplicado correctamente.
                return Created("", new { message = "Bloqueo de disponibilidad aplicado correctamente." });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-009"))
            {
                return BadRequest(ex.Message); // G-ERROR-009: Bloqueo por Citas (Estricto)
            }
        }

        // PEL-HU-13: Descansos Fijos Diarios (Almuerzo/Pausa)
        [HttpPut("{estilistaId:int}/descanso-fijo")]
        public async Task<IActionResult> UpdateDescansoFijo(int estilistaId, [FromBody] List<HorarioDiaDto> descansos)
        {
            if (!ModelState.IsValid || descansos == null || descansos.Count == 0)
            {
                return BadRequest("Debe proporcionar una lista válida de descansos.");
            }

            // TODO: Antes de llamar al servicio, se haría la validación RNI-H004 (Única Pausa Diaria)

            try
            {
                var success = await _agendaService.UpdateDescansoFijoAsync(estilistaId, descansos);

                if (!success) return NotFound("Estilista no encontrado.");

                // INFO-HU13-01: Bloqueo de disponibilidad aplicado correctamente.
                return Ok(new { message = "Descansos fijos actualizados correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{estilistaId:int}/horario-base")]
        public async Task<IActionResult> GetHorarioBase(int estilistaId)
        {
            var horarios = await _agendaService.GetHorarioBaseAsync(estilistaId);

            // Retorna la lista de horarios (vacía si no hay configuración)
            return Ok(horarios);
        }

        // PEL-HU-13: Consultar Bloqueos de Días Libres (GET)
        [HttpGet("{estilistaId:int}/bloqueo-dias-libres")]
        public async Task<IActionResult> GetBloqueosDiasLibres(int estilistaId)
        {
            var bloqueos = await _agendaService.GetBloqueosDiasLibresAsync(estilistaId);

            return Ok(bloqueos);
        }

        // PEL-HU-13: Consultar Descansos Fijos Diarios (GET)
        [HttpGet("{estilistaId:int}/descanso-fijo")]
        public async Task<IActionResult> GetDescansosFijos(int estilistaId)
        {
            var descansos = await _agendaService.GetDescansosFijosAsync(estilistaId);

            return Ok(descansos);
        }
    }
}