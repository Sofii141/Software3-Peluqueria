using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Categoria;

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
            var newCategoria = await _categoriaService.CreateAsync(requestDto);
            return CreatedAtAction(nameof(GetAll), new { id = newCategoria.Id }, newCategoria);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaRequestDto requestDto)
        {
            var updatedCategoria = await _categoriaService.UpdateAsync(id, requestDto);
            return Ok(updatedCategoria);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inactivate(int id)
        {
            await _categoriaService.InactivateAsync(id);
            return NoContent();
        }
    }
}