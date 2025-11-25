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
            var nuevoServicioDto = await _servicioService.CreateAsync(requestDto);

            AsignarUrlImagen(nuevoServicioDto);

            return CreatedAtAction(nameof(GetById), new { id = nuevoServicioDto.Id }, nuevoServicioDto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateServicioRequestDto requestDto)
        {
            var servicioActualizadoDto = await _servicioService.UpdateAsync(id, requestDto);

            if (servicioActualizadoDto == null) return NotFound();

            AsignarUrlImagen(servicioActualizadoDto);

            return Ok(servicioActualizadoDto);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inactivate(int id)
        {
            await _servicioService.InactivateAsync(id);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var servicios = await _servicioService.GetAllAsync();

            foreach (var servicio in servicios)
            {
                AsignarUrlImagen(servicio);
            }
            return Ok(servicios);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var servicio = await _servicioService.GetByIdAsync(id);

            if (servicio == null) return NotFound();

            AsignarUrlImagen(servicio);

            return Ok(servicio);
        }

        [HttpGet("categoria/{categoriaId:int}")]
        public async Task<IActionResult> GetByCategoria(int categoriaId)
        {
            var servicios = await _servicioService.GetByCategoriaIdAsync(categoriaId);

            foreach (var servicio in servicios)
            {
                AsignarUrlImagen(servicio);
            }
            return Ok(servicios);
        }

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