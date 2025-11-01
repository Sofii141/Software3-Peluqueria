// Peluqueria.Application/Services/ServicioService.cs
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

        // Lógica de Presentación (requestScheme, requestHost) ELIMINADA.
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

            // Lógica de Negocio: Manejo de archivos y guardado del nombre único (CORRECTO)
            if (requestDto.Imagen != null && requestDto.Imagen.Length > 0)
            {
                var nombreArchivo = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images");
                // SOLO se guarda el nombre del archivo, no la URL absoluta.
                servicio.Imagen = nombreArchivo;
            }

            var nuevoServicio = await _servicioRepo.CreateAsync(servicio);
            // Si el repositorio incluye la categoría, el mapeo funciona correctamente.
            var servicioCompleto = await _servicioRepo.GetByIdAsync(nuevoServicio.Id);

            return MapToDto(servicioCompleto!);
        }

        // El resto de los métodos (GetAllAsync, GetByIdAsync, GetByCategoriaIdAsync, DeleteAsync) se mantienen igual...
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

        // Lógica de Presentación (requestScheme, requestHost) ELIMINADA.
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

            // Se guarda SOLO el nombre de archivo en la entidad.
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

            // Aquí el DTO tendrá el nombre de archivo interno
            return MapToDto(servicioActualizado);
        }

        private static ServicioDto MapToDto(Servicio servicio)
        {
            // El mapeo permanece igual, Imagen contendrá solo el nombre del archivo.
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