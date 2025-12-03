using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Categoria;

namespace Peluqueria.API.Controllers
{
    /// <summary>
    /// Controlador encargado de la gestión del catálogo de categorías de servicios.
    /// Permite listar, crear, modificar e inactivar categorías.
    /// </summary>
    [Route("api/categorias")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CategoriasController"/>.
        /// </summary>
        /// <param name="categoriaService">Servicio de aplicación para la lógica de negocio de categorías.</param>
        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Obtiene el listado completo de todas las categorías registradas en el sistema.
        /// </summary>
        /// <returns>Una colección de categorías disponibles.</returns>
        /// <response code="200">Devuelve la lista de categorías.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoriaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categoriaDtos = await _categoriaService.GetAllAsync();
            return Ok(categoriaDtos);
        }

        /// <summary>
        /// Crea una nueva categoría en el catálogo.
        /// </summary>
        /// <remarks>
        /// Requiere permisos de Administrador.
        /// </remarks>
        /// <param name="requestDto">Objeto con el nombre de la nueva categoría.</param>
        /// <returns>La categoría recién creada con su ID asignado.</returns>
        /// <response code="201">Si la categoría se creó exitosamente.</response>
        /// <response code="400">Si los datos son inválidos.</response>
        /// <response code="401">Si el usuario no está autenticado.</response>
        /// <response code="403">Si el usuario no tiene rol de Administrador.</response>
        /// <response code="409">Si ya existe una categoría con ese nombre.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCategoriaRequestDto requestDto)
        {
            var newCategoria = await _categoriaService.CreateAsync(requestDto);
            return CreatedAtAction(nameof(GetAll), new { id = newCategoria.Id }, newCategoria);
        }

        /// <summary>
        /// Actualiza la información de una categoría existente.
        /// </summary>
        /// <remarks>
        /// Requiere permisos de Administrador.
        /// </remarks>
        /// <param name="id">Identificador único de la categoría a modificar.</param>
        /// <param name="requestDto">Objeto con los nuevos datos de la categoría.</param>
        /// <returns>La categoría actualizada.</returns>
        /// <response code="200">Si la actualización fue exitosa.</response>
        /// <response code="404">Si la categoría no existe.</response>
        /// <response code="409">Si el nuevo nombre genera conflicto con otra categoría existente.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaRequestDto requestDto)
        {
            var updatedCategoria = await _categoriaService.UpdateAsync(id, requestDto);
            return Ok(updatedCategoria);
        }

        /// <summary>
        /// Realiza la baja lógica (inactivación) de una categoría.
        /// </summary>
        /// <remarks>
        /// Requiere permisos de Administrador. 
        /// La operación será rechazada si existen servicios activos o reservaciones futuras asociadas a esta categoría.
        /// </remarks>
        /// <param name="id">Identificador único de la categoría a inactivar.</param>
        /// <returns>Sin contenido.</returns>
        /// <response code="204">Si la categoría se inactivó correctamente.</response>
        /// <response code="400">Si existen reglas de negocio que impiden la inactivación (ej. reservaciones asociadas).</response>
        /// <response code="404">Si la categoría no existe.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Inactivate(int id)
        {
            await _categoriaService.InactivateAsync(id);
            return NoContent();
        }
    }
}