using Microsoft.AspNetCore.Mvc;
using Peluqueria.Application.Interfaces;

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
    }
}