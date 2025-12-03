using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Servicio;

namespace Peluqueria.API.Controllers
{
    /// <summary>
    /// Controlador encargado de la gestión del catálogo de servicios ofrecidos por la peluquería.
    /// Permite crear, actualizar, consultar e inactivar servicios, incluyendo la gestión de imágenes.
    /// </summary>
    [Route("api/servicios")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly IServicioService _servicioService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ServiciosController"/>.
        /// </summary>
        /// <param name="servicioService">Servicio de aplicación para la lógica de negocio de servicios.</param>
        public ServiciosController(IServicioService servicioService)
        {
            _servicioService = servicioService;
        }

        /// <summary>
        /// Crea un nuevo servicio en el catálogo.
        /// </summary>
        /// <remarks>
        /// Requiere rol de Administrador. Los datos se reciben mediante `multipart/form-data` para permitir la subida de una imagen ilustrativa.
        /// </remarks>
        /// <param name="requestDto">DTO con los datos del servicio (nombre, precio, duración, etc.) y el archivo de imagen.</param>
        /// <returns>El servicio recién creado con su URL de imagen procesada.</returns>
        /// <response code="201">Si el servicio fue creado exitosamente.</response>
        /// <response code="400">Si los datos son inválidos o se violan reglas de negocio (ej. duración o precio incorrectos).</response>
        /// <response code="409">Si ya existe un servicio con el mismo nombre.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServicioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromForm] CreateServicioRequestDto requestDto)
        {
            var nuevoServicioDto = await _servicioService.CreateAsync(requestDto);

            AsignarUrlImagen(nuevoServicioDto);

            return CreatedAtAction(nameof(GetById), new { id = nuevoServicioDto.Id }, nuevoServicioDto);
        }

        /// <summary>
        /// Actualiza la información de un servicio existente.
        /// </summary>
        /// <remarks>
        /// Requiere rol de Administrador. Permite modificar detalles del servicio y/o reemplazar su imagen.
        /// </remarks>
        /// <param name="id">Identificador único del servicio a actualizar.</param>
        /// <param name="requestDto">DTO con los nuevos datos del servicio.</param>
        /// <returns>El servicio actualizado.</returns>
        /// <response code="200">Si la actualización fue exitosa.</response>
        /// <response code="404">Si el servicio no existe.</response>
        /// <response code="400">Si existen reglas de negocio que impiden la actualización (ej. reservaciones futuras activas).</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServicioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateServicioRequestDto requestDto)
        {
            var servicioActualizadoDto = await _servicioService.UpdateAsync(id, requestDto);

            AsignarUrlImagen(servicioActualizadoDto);

            return Ok(servicioActualizadoDto);
        }

        /// <summary>
        /// Realiza la baja lógica (inactivación) de un servicio.
        /// </summary>
        /// <remarks>
        /// Requiere rol de Administrador. La operación será bloqueada si existen reservaciones futuras pendientes para este servicio.
        /// </remarks>
        /// <param name="id">Identificador del servicio.</param>
        /// <returns>Sin contenido.</returns>
        /// <response code="204">Si el servicio fue inactivado correctamente.</response>
        /// <response code="400">Si existen reservaciones que impiden la inactivación.</response>
        /// <response code="404">Si el servicio no existe.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Inactivate(int id)
        {
            await _servicioService.InactivateAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Obtiene el listado completo de servicios disponibles.
        /// </summary>
        /// <returns>Colección de servicios con sus URLs de imagen absolutas.</returns>
        /// <response code="200">Retorna la lista de servicios.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServicioDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var servicios = await _servicioService.GetAllAsync();

            foreach (var servicio in servicios)
            {
                AsignarUrlImagen(servicio);
            }
            return Ok(servicios);
        }

        /// <summary>
        /// Obtiene los detalles de un servicio específico por su ID.
        /// </summary>
        /// <param name="id">Identificador único del servicio.</param>
        /// <returns>Detalles del servicio.</returns>
        /// <response code="200">Retorna el servicio solicitado.</response>
        /// <response code="404">Si el servicio no existe.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ServicioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var servicio = await _servicioService.GetByIdAsync(id);

            AsignarUrlImagen(servicio);

            return Ok(servicio);
        }

        /// <summary>
        /// Obtiene todos los servicios asociados a una categoría específica.
        /// </summary>
        /// <param name="categoriaId">Identificador de la categoría.</param>
        /// <returns>Lista de servicios filtrados por categoría.</returns>
        /// <response code="200">Retorna la lista de servicios de la categoría.</response>
        /// <response code="404">Si la categoría no existe.</response>
        [HttpGet("categoria/{categoriaId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ServicioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCategoria(int categoriaId)
        {
            var servicios = await _servicioService.GetByCategoriaIdAsync(categoriaId);

            foreach (var servicio in servicios)
            {
                AsignarUrlImagen(servicio);
            }
            return Ok(servicios);
        }

        /// <summary>
        /// Método auxiliar para construir la URL absoluta de la imagen del servicio basada en el host actual.
        /// </summary>
        private void AsignarUrlImagen(ServicioDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Imagen))
            {
                if (!dto.Imagen.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    dto.Imagen = $"{Request.Scheme}://{Request.Host}/images/{dto.Imagen}";
                }
            }
        }
    }
}