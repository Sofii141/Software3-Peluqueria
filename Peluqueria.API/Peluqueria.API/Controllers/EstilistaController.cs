using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.API.Controllers
{

    [Route("api/estilistas")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EstilistaController : ControllerBase
    {
        private readonly IEstilistaService _estilistaService;

        public EstilistaController(IEstilistaService estilistaService)
        {
            _estilistaService = estilistaService;
        }

        // PEL-HU-09: Crear Estilista
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEstilistaRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // G-ERROR-002: Campos Vacíos
            }

            try
            {
                var newEstilista = await _estilistaService.CreateAsync(requestDto);
                // INFO-HU09-01: Estilista [Nombre] registrado
                return CreatedAtAction(nameof(GetById), new { id = newEstilista.Id }, newEstilista);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-006"))
            {
                return BadRequest(ex.Message); // G-ERROR-006: Servicios Mínimos
            }
            catch (Exception ex) when (ex.Message.Contains("Fallo en la creación de credenciales"))
            {
                // Este manejo de excepciones es necesario porque G-ERROR-005 (Correo/Usuario duplicado)
                // se genera en la capa de Identity, y lo propagamos a través del EstilistaService.
                return BadRequest("El correo/usuario ya se encuentra registrado. (G-ERROR-005)");
            }
        }

        // Asumiendo que existe un GetById para CreatedAtAction
        // [HttpGet("{id:int}")]
        // public async Task<IActionResult> GetById(int id) { ... }


        // PEL-HU-10: Modificar Estilista
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEstilistaRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // G-ERROR-002: Campos Obligatorios
            }

            // TODO: Antes de llamar al servicio, se haría la validación del bloqueo de citas futuras (G-ERROR-009)

            try
            {
                var updatedEstilista = await _estilistaService.UpdateAsync(id, requestDto);

                if (updatedEstilista == null) return NotFound();

                // INFO-HU10-01: Información del Estilista actualizada correctamente.
                return Ok(updatedEstilista);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-006"))
            {
                return BadRequest(ex.Message); // G-ERROR-006: Servicios Mínimos
            }
            catch (Exception ex)
            {
                // Este catch es genérico para el ejemplo, pero debe ser más específico si se implementa G-ERROR-005 (Unicidad) en Update.
                return StatusCode(500, "Error interno al actualizar el estilista.");
            }
        }

        // PEL-HU-11: Inactivar Estilista
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Inactivate(int id)
        {
            // TODO: Antes de llamar al servicio, se haría la validación del bloqueo de citas futuras (G-ERROR-009)

            try
            {
                var success = await _estilistaService.InactivateAsync(id);

                if (!success) return NotFound();

                // INFO-HU11-01: Estilista inactivado exitosamente. Ya no podrá recibir nuevas citas.
                // Usamos 204 No Content para la baja lógica.
                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-009"))
            {
                return BadRequest(ex.Message); // G-ERROR-009: Bloqueo por Citas Futuras
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var estilistas = await _estilistaService.GetAllAsync();
            return Ok(estilistas); // PROT-HU08-01
        }

        // Usado por CreatedAtAction y por consulta directa
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var estilista = await _estilistaService.GetByIdAsync(id);
            if (estilista == null) return NotFound();
            return Ok(estilista);
        }


    }
}