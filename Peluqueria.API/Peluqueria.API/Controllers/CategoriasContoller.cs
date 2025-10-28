using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Categoria;

namespace Peluqueria.API.Controllers
{
    [Route("api/categorias")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _categoriaRepo;

        public CategoriasController(ICategoriaRepository categoriaRepo)
        {
            _categoriaRepo = categoriaRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categorias = await _categoriaRepo.GetAllAsync();
            var categoriaDtos = categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre
            });
            return Ok(categoriaDtos);
        }
    }
}