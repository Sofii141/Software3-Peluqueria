// Peluqueria.API/Controllers/CategoriasController.cs
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
        // Se reemplaza ICategoriaRepository por ICategoriaService
        private readonly ICategoriaService _categoriaService;

        // El constructor inyecta el nuevo Servicio de Aplicación
        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Toda la lógica (acceso a repo y mapeo) se delega al servicio
            var categoriaDtos = await _categoriaService.GetAllAsync();

            // El controlador solo devuelve el resultado HTTP
            return Ok(categoriaDtos);
        }
    }
}