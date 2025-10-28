using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Application.Dtos.Servicio;
using Peluqueria.Application.Dtos.Categoria;

namespace Peluqueria.API.Controllers
{
    [Route("api/servicios")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly IServicioRepository _servicioRepo;
        private readonly IFileStorageService _fileStorage;

        public ServiciosController(IServicioRepository servicioRepo, IFileStorageService fileStorage)
        {
            _servicioRepo = servicioRepo;
            _fileStorage = fileStorage;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateServicioRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var servicio = new Servicio
            {
                Nombre = requestDto.Nombre,
                Descripcion = requestDto.Descripcion,
                Precio = requestDto.Precio,
                Disponible = requestDto.Disponible,
                CategoriaId = requestDto.CategoriaId,
                FechaCreacion = DateTime.UtcNow
            };

            if (requestDto.Imagen != null && requestDto.Imagen.Length > 0)
            {
                var nombreArchivo = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images");
                servicio.Imagen = $"{Request.Scheme}://{Request.Host}/images/{nombreArchivo}";
            }

            var nuevoServicio = await _servicioRepo.CreateAsync(servicio);
            var servicioCompleto = await _servicioRepo.GetByIdAsync(nuevoServicio.Id);

            return CreatedAtAction(nameof(GetById), new { id = servicioCompleto!.Id }, MapToDto(servicioCompleto));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateServicioRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscamos el servicio existente
            var servicioExistente = await _servicioRepo.GetByIdAsync(id);
            if (servicioExistente == null)
            {
                return NotFound();
            }

            // Actualizamos las propiedades
            servicioExistente.Nombre = requestDto.Nombre;
            servicioExistente.Descripcion = requestDto.Descripcion;
            servicioExistente.Precio = requestDto.Precio;
            servicioExistente.Disponible = requestDto.Disponible;
            servicioExistente.CategoriaId = requestDto.CategoriaId;

            // Si el usuario ha subido una nueva imagen, la guardamos y actualizamos la URL
            if (requestDto.Imagen != null && requestDto.Imagen.Length > 0)
            {
                var nombreArchivo = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images");
                servicioExistente.Imagen = $"{Request.Scheme}://{Request.Host}/images/{nombreArchivo}";
            }

            // Llamamos al método UpdateAsync del repositorio
            var servicioActualizado = await _servicioRepo.UpdateAsync(id, servicioExistente);

            if (servicioActualizado == null)
            {
                return NotFound(); // En caso de que algo falle en el último segundo
            }

            // Devolvemos el servicio actualizado mapeado a su DTO
            return Ok(MapToDto(servicioActualizado));
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var servicios = await _servicioRepo.GetAllAsync();
            return Ok(servicios.Select(MapToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var servicio = await _servicioRepo.GetByIdAsync(id);
            if (servicio == null) return NotFound();
            return Ok(MapToDto(servicio));
        }

        [HttpGet("categoria/{categoriaId:int}")]
        public async Task<IActionResult> GetByCategoria(int categoriaId)
        {
            var servicios = await _servicioRepo.GetByCategoriaIdAsync(categoriaId);
            return Ok(servicios.Select(MapToDto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _servicioRepo.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        private static ServicioDto MapToDto(Servicio servicio)
        {
            return new()
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                Descripcion = servicio.Descripcion,
                Precio = servicio.Precio,
                Imagen = servicio.Imagen,
                FechaCreacion = servicio.FechaCreacion,
                Disponible = servicio.Disponible,
                Categoria = servicio.Categoria != null ? new CategoriaDto
                {
                    Id = servicio.Categoria.Id,
                    Nombre = servicio.Categoria.Nombre
                } : null!
            };
        }
    }
}