using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Dtos.Servicio;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Services
{
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _servicioRepo;
        private readonly IFileStorageService _fileStorage;

        public ServicioService(IServicioRepository servicioRepo, IFileStorageService fileStorage)
        {
            _servicioRepo = servicioRepo;
            _fileStorage = fileStorage;
        }

        public async Task<ServicioDto> CreateAsync(CreateServicioRequestDto requestDto)
        {
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
                servicio.Imagen = nombreArchivo;
            }

            var nuevoServicio = await _servicioRepo.CreateAsync(servicio);
            var servicioCompleto = await _servicioRepo.GetByIdAsync(nuevoServicio.Id);

            return MapToDto(servicioCompleto!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _servicioRepo.DeleteAsync(id);
        }

        public async Task<IEnumerable<ServicioDto>> GetAllAsync()
        {
            var servicios = await _servicioRepo.GetAllAsync();
            return servicios.Select(MapToDto);
        }

        public async Task<IEnumerable<ServicioDto>> GetByCategoriaIdAsync(int categoriaId)
        {
            var servicios = await _servicioRepo.GetByCategoriaIdAsync(categoriaId);
            return servicios.Select(MapToDto);
        }

        public async Task<ServicioDto?> GetByIdAsync(int id)
        {
            var servicio = await _servicioRepo.GetByIdAsync(id);
            return servicio == null ? null : MapToDto(servicio);
        }

        public async Task<ServicioDto?> UpdateAsync(int id, UpdateServicioRequestDto requestDto)
        {
            var servicioExistente = await _servicioRepo.GetByIdAsync(id);
            if (servicioExistente == null)
            {
                return null;
            }

            servicioExistente.Nombre = requestDto.Nombre;
            servicioExistente.Descripcion = requestDto.Descripcion;
            servicioExistente.Precio = requestDto.Precio;
            servicioExistente.Disponible = requestDto.Disponible;
            servicioExistente.CategoriaId = requestDto.CategoriaId;

            if (requestDto.Imagen != null && requestDto.Imagen.Length > 0)
            {
                var nombreArchivo = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images");
                servicioExistente.Imagen = nombreArchivo;
            }

            var servicioActualizado = await _servicioRepo.UpdateAsync(id, servicioExistente);

            if (servicioActualizado == null)
            {
                return null;
            }

            return MapToDto(servicioActualizado);
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