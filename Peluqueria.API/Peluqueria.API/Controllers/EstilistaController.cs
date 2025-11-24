using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.API.Controllers
{
    [Route("api/estilistas")]
    [ApiController]
    public class EstilistaController : ControllerBase
    {
        private readonly IEstilistaService _estilistaService;

        public EstilistaController(IEstilistaService estilistaService)
        {
            _estilistaService = estilistaService;
        }

        private void SetImageUrl(EstilistaDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Imagen))
            {
                dto.Imagen = $"{Request.Scheme}://{Request.Host}/images/estilistas/{dto.Imagen}";
            }
        }

        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> GetAll()
        {
            var estilistas = await _estilistaService.GetAllAsync();
            estilistas.ToList().ForEach(SetImageUrl);
            return Ok(estilistas);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var estilista = await _estilistaService.GetByIdAsync(id);
            if (estilista == null) return NotFound();

            SetImageUrl(estilista);

            return Ok(estilista);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateEstilistaRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newEstilista = await _estilistaService.CreateAsync(requestDto);

                SetImageUrl(newEstilista); 

                return CreatedAtAction(nameof(GetById), new { id = newEstilista.Id }, newEstilista);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-006"))
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception ex) when (ex.Message.Contains("Fallo en la creación de credenciales"))
            {
                return BadRequest("El correo/usuario ya se encuentra registrado. (G-ERROR-005)");
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateEstilistaRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            try
            {
                var updatedEstilista = await _estilistaService.UpdateAsync(id, requestDto);

                if (updatedEstilista == null) return NotFound();

                SetImageUrl(updatedEstilista); 

                return Ok(updatedEstilista);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-006"))
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno al actualizar el estilista.");
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inactivate(int id)
        {
            try
            {
                var success = await _estilistaService.InactivateAsync(id);

                if (!success) return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-009"))
            {
                return BadRequest(ex.Message); 
            }
        }
    }
}