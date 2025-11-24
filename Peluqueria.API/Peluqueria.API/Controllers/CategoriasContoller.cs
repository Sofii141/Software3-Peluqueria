using Microsoft.AspNetCore.Mvc;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Categoria;
using Microsoft.AspNetCore.Authorization;

namespace Peluqueria.API.Controllers
{
    [Route("api/categorias")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categoriaDtos = await _categoriaService.GetAllAsync();

            return Ok(categoriaDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoriaRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // G-ERROR-002: Campos Obligatorios
            }
            try
            {
                var newCategoria = await _categoriaService.CreateAsync(requestDto);
                return CreatedAtAction(nameof(GetAll), new { id = newCategoria.Id }, newCategoria);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-007"))
            {
                return BadRequest(ex.Message); // G-ERROR-007: Categoría ya existe
            }
        }

        // PEL-HU-22: Actualizar
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // G-ERROR-002: Campos Obligatorios
            }
            try
            {
                var updatedCategoria = await _categoriaService.UpdateAsync(id, requestDto);
                if (updatedCategoria == null) return NotFound();
                return Ok(updatedCategoria);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-007"))
            {
                return BadRequest(ex.Message); // G-ERROR-007: Categoría ya existe (duplicado)
            }
            // NOTA: La validación G-ERROR-008 (Bloqueo por Servicios Asociados) debería capturarse aquí si la implementas en el servicio.
        }

        // PEL-HU-23: Inactivar
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inactivate(int id)
        {
            try
            {
                var success = await _categoriaService.InactivateAsync(id);
                if (!success) return NotFound();
                // Retorna 204 No Content para indicar baja lógica exitosa.
                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("G-ERROR-008"))
            {
                return BadRequest(ex.Message); // G-ERROR-008: Bloqueo por Servicios Asociados
            }
        }
    }
}