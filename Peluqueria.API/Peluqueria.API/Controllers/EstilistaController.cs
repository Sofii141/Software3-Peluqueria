using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.API.Controllers
{
    /// <summary>
    /// Controlador encargado de la gestión de la información de los estilistas.
    /// Permite la consulta pública de estilistas y la administración (creación, edición, baja) restringida a administradores.
    /// </summary>
    [Route("api/estilistas")]
    [ApiController]
    public class EstilistaController : ControllerBase
    {
        private readonly IEstilistaService _estilistaService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EstilistaController"/>.
        /// </summary>
        /// <param name="estilistaService">Servicio de aplicación para la gestión de estilistas.</param>
        public EstilistaController(IEstilistaService estilistaService)
        {
            _estilistaService = estilistaService;
        }

        /// <summary>
        /// Obtiene el listado completo de estilistas registrados en el sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint es público (Anónimo) para permitir que los clientes visualicen el equipo de trabajo.
        /// </remarks>
        /// <returns>Colección de estilistas con sus URLs de imagen procesadas.</returns>
        /// <response code="200">Retorna la lista de estilistas.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<EstilistaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var estilistas = await _estilistaService.GetAllAsync();

            var lista = estilistas.ToList();
            lista.ForEach(SetImageUrl);

            return Ok(lista);
        }

        /// <summary>
        /// Obtiene los detalles de un estilista específico por su ID.
        /// </summary>
        /// <param name="id">Identificador único del estilista.</param>
        /// <returns>Datos detallados del estilista.</returns>
        /// <response code="200">Retorna el estilista solicitado.</response>
        /// <response code="404">Si el estilista no existe.</response>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EstilistaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var estilista = await _estilistaService.GetByIdAsync(id);

            SetImageUrl(estilista);
            return Ok(estilista);
        }

        /// <summary>
        /// Registra un nuevo estilista en el sistema.
        /// </summary>
        /// <remarks>
        /// Requiere rol de Administrador. Los datos se reciben mediante `multipart/form-data` para permitir la subida de la imagen de perfil.
        /// </remarks>
        /// <param name="requestDto">DTO con los datos del estilista y el archivo de imagen opcional.</param>
        /// <returns>El estilista creado con su ID asignado.</returns>
        /// <response code="201">Si el estilista fue creado exitosamente.</response>
        /// <response code="400">Si los datos son inválidos o faltan campos requeridos.</response>
        /// <response code="409">Si el usuario o correo ya existen en el sistema Identity.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EstilistaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromForm] CreateEstilistaRequestDto requestDto)
        {
            var newEstilista = await _estilistaService.CreateAsync(requestDto);

            SetImageUrl(newEstilista);

            return CreatedAtAction(nameof(GetById), new { id = newEstilista.Id }, newEstilista);
        }

        /// <summary>
        /// Actualiza la información de un estilista existente.
        /// </summary>
        /// <remarks>
        /// Requiere rol de Administrador. Permite actualizar datos personales, credenciales y/o imagen.
        /// </remarks>
        /// <param name="id">Identificador del estilista a actualizar.</param>
        /// <param name="requestDto">DTO con los datos actualizados.</param>
        /// <returns>El estilista actualizado.</returns>
        /// <response code="200">Si la actualización fue exitosa.</response>
        /// <response code="404">Si el estilista no existe.</response>
        /// <response code="400">Si hay errores de validación (ej. contraseña débil).</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EstilistaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateEstilistaRequestDto requestDto)
        {
            var updatedEstilista = await _estilistaService.UpdateAsync(id, requestDto);

            SetImageUrl(updatedEstilista);

            return Ok(updatedEstilista);
        }

        /// <summary>
        /// Realiza la baja lógica (inactivación) de un estilista.
        /// </summary>
        /// <remarks>
        /// Requiere rol de Administrador. La operación fallará si el estilista tiene citas futuras pendientes.
        /// </remarks>
        /// <param name="id">Identificador del estilista.</param>
        /// <returns>Sin contenido.</returns>
        /// <response code="204">Si la inactivación fue exitosa.</response>
        /// <response code="400">Si existen reglas de negocio que impiden la baja (ej. citas pendientes).</response>
        /// <response code="404">Si el estilista no existe.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Inactivate(int id)
        {
            await _estilistaService.InactivateAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Método auxiliar para construir la URL absoluta de la imagen del estilista basada en el host actual.
        /// </summary>
        private void SetImageUrl(EstilistaDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Imagen))
            {
                if (!dto.Imagen.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    dto.Imagen = $"{Request.Scheme}://{Request.Host}/images/estilistas/{dto.Imagen}";
                }
            }
        }
    }
}