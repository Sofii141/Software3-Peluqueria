using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Dtos.Servicio;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using System.Globalization;

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
            if (!TryConvertPrecio(requestDto.Precio, out double precioValor))
            {
                throw new ArgumentException("El precio debe ser un valor numérico válido mayor o igual a 1. Utiliza el punto como separador decimal (ej: 50000.00).");
            }

            var servicio = new Servicio
            {
                Nombre = requestDto.Nombre,
                Descripcion = requestDto.Descripcion,
                Precio = precioValor,
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
            // La validación ahora funciona incluso si requestDto.Precio es null.
            if (!TryConvertPrecio(requestDto.Precio, out double precioValor))
            {
                throw new ArgumentException("El precio debe ser un valor numérico válido mayor o igual a 1. Utiliza el punto como separador decimal (ej: 50000.00).");
            }

            var servicioExistente = await _servicioRepo.GetByIdAsync(id);
            if (servicioExistente == null)
            {
                return null;
            }

            servicioExistente.Nombre = requestDto.Nombre;
            servicioExistente.Descripcion = requestDto.Descripcion;
            servicioExistente.Precio = precioValor;
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

        private bool TryConvertPrecio(string? precioString, out double precioValor)
        {
            precioValor = 0;
            // CORRECCIÓN: Usamos IsNullOrWhiteSpace para cubrir null, "" o "   "
            if (string.IsNullOrWhiteSpace(precioString)) return false;

            // Reemplazamos coma (,) por punto (.) para estandarizar el formato decimal
            string cleanedString = precioString.Replace(',', '.');

            // Intentamos el parseo con la cultura invariante (usa siempre el punto como decimal)
            if (double.TryParse(cleanedString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) && result >= 1)
            {
                precioValor = result;
                return true;
            }

            return false;
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
