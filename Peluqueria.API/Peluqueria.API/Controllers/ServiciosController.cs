using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Servicio;

namespace Peluqueria.API.Controllers
{
    [Route("api/servicios")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly IServicioService _servicioService;

        public ServiciosController(IServicioService servicioService)
        {
            _servicioService = servicioService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateServicioRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Delega la lógica de negocio (creación de entidad, manejo de archivo) al servicio
            // Ya no se pasan requestScheme ni requestHost.
            var nuevoServicioDto = await _servicioService.CreateAsync(requestDto);

            // LÓGICA DE PRESENTACIÓN: Construcción de la URL absoluta aquí.
            // El DTO del servicio tiene solo el nombre del archivo.
            if (!string.IsNullOrEmpty(nuevoServicioDto.Imagen))
            {
                nuevoServicioDto.Imagen = $"{Request.Scheme}://{Request.Host}/images/{nuevoServicioDto.Imagen}";
            }

            // El controlador solo se encarga del resultado HTTP
            return CreatedAtAction(nameof(GetById), new { id = nuevoServicioDto.Id }, nuevoServicioDto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateServicioRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Delega toda la lógica de actualización al servicio.
            // Ya no se pasan requestScheme ni requestHost.
            var servicioActualizadoDto = await _servicioService.UpdateAsync(id, requestDto);

            if (servicioActualizadoDto == null)
            {
                return NotFound();
            }

            // LÓGICA DE PRESENTACIÓN: Construcción de la URL absoluta aquí.
            if (!string.IsNullOrEmpty(servicioActualizadoDto.Imagen))
            {
                servicioActualizadoDto.Imagen = $"{Request.Scheme}://{Request.Host}/images/{servicioActualizadoDto.Imagen}";
            }

            // El controlador solo se encarga del resultado HTTP
            return Ok(servicioActualizadoDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var servicios = await _servicioService.GetAllAsync();

            // LÓGICA DE PRESENTACIÓN: Mapeo de URL para todos los elementos
            foreach (var servicio in servicios)
            {
                if (!string.IsNullOrEmpty(servicio.Imagen))
                {
                    servicio.Imagen = $"{Request.Scheme}://{Request.Host}/images/{servicio.Imagen}";
                }
            }
            return Ok(servicios);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var servicio = await _servicioService.GetByIdAsync(id);
            if (servicio == null) return NotFound();

            // LÓGICA DE PRESENTACIÓN: Mapeo de URL
            if (!string.IsNullOrEmpty(servicio.Imagen))
            {
                servicio.Imagen = $"{Request.Scheme}://{Request.Host}/images/{servicio.Imagen}";
            }

            return Ok(servicio);
        }

        [HttpGet("categoria/{categoriaId:int}")]
        public async Task<IActionResult> GetByCategoria(int categoriaId)
        {
            var servicios = await _servicioService.GetByCategoriaIdAsync(categoriaId);

            // LÓGICA DE PRESENTACIÓN: Mapeo de URL para todos los elementos
            foreach (var servicio in servicios)
            {
                if (!string.IsNullOrEmpty(servicio.Imagen))
                {
                    servicio.Imagen = $"{Request.Scheme}://{Request.Host}/images/{servicio.Imagen}";
                }
            }
            return Ok(servicios);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // La eliminación es pura lógica de negocio y persistencia.
            var success = await _servicioService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}