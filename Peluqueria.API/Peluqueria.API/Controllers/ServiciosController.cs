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

            try
            {
                var nuevoServicioDto = await _servicioService.CreateAsync(requestDto);

                if (!string.IsNullOrEmpty(nuevoServicioDto.Imagen))
                {
                    nuevoServicioDto.Imagen = $"{Request.Scheme}://{Request.Host}/images/{nuevoServicioDto.Imagen}";
                }

                return CreatedAtAction(nameof(GetById), new { id = nuevoServicioDto.Id }, nuevoServicioDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateServicioRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var servicioActualizadoDto = await _servicioService.UpdateAsync(id, requestDto);

                if (servicioActualizadoDto == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(servicioActualizadoDto.Imagen))
                {
                    servicioActualizadoDto.Imagen = $"{Request.Scheme}://{Request.Host}/images/{servicioActualizadoDto.Imagen}";
                }

                return Ok(servicioActualizadoDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var servicios = await _servicioService.GetAllAsync();

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

            if (!string.IsNullOrEmpty(servicio.Imagen))
            {
                if (!servicio.Imagen.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !servicio.Imagen.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    servicio.Imagen = $"{Request.Scheme}://{Request.Host}/images/{servicio.Imagen}";
                }
            }

            return Ok(servicio);
        }


        [HttpGet("categoria/{categoriaId:int}")]
        public async Task<IActionResult> GetByCategoria(int categoriaId)
        {
            var servicios = await _servicioService.GetByCategoriaIdAsync(categoriaId);

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
        public async Task<IActionResult> Inactivate(int id)
        {
            var success = await _servicioService.InactivateAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}