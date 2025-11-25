using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Estilista;

namespace Peluqueria.API.Controllers
{
    [Route("api/estilistas")]
    [ApiController]
    public class EstilistaController : ControllerBase
    {
        private readonly IEstilistaService _estilistaService;

        public EstilistaController(IEstilistaService estilistaService)
        {
            _estilistaService = estilistaService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var estilistas = await _estilistaService.GetAllAsync();

            var lista = estilistas.ToList();
            lista.ForEach(SetImageUrl);

            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var estilista = await _estilistaService.GetByIdAsync(id);
            if (estilista == null) return NotFound();

            SetImageUrl(estilista);
            return Ok(estilista);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateEstilistaRequestDto requestDto)
        {
            var newEstilista = await _estilistaService.CreateAsync(requestDto);

            SetImageUrl(newEstilista);

            return CreatedAtAction(nameof(GetById), new { id = newEstilista.Id }, newEstilista);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateEstilistaRequestDto requestDto)
        {
            var updatedEstilista = await _estilistaService.UpdateAsync(id, requestDto);

            if (updatedEstilista == null) return NotFound();

            SetImageUrl(updatedEstilista);

            return Ok(updatedEstilista);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inactivate(int id)
        {
            await _estilistaService.InactivateAsync(id);

            return NoContent();
        }

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