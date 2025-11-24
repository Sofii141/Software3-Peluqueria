using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Dtos.Servicio;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using System.Globalization;
using Peluqueria.Application.Dtos.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peluqueria.Application.Services
{
    public class ServicioService : IServicioService
    {

        private readonly IServicioRepository _servicioRepo;
        private readonly IFileStorageService _fileStorage;
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IMessagePublisher _messagePublisher;

        public ServicioService(IServicioRepository servicioRepo, IFileStorageService fileStorage
            , ICategoriaRepository categoriaRepo, IMessagePublisher messagePublisher)
        {
            _servicioRepo = servicioRepo;
            _fileStorage = fileStorage;
            _categoriaRepo = categoriaRepo;
            _messagePublisher = messagePublisher;
        }

        // --- MÉTODO CORREGIDO: PUBLISHSERVICIOSERVICE ---
        private Task PublishServicioEvent(Servicio servicio, string accion)
        {
            var evento = new ServicioEventDto
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                DuracionMinutos = servicio.DuracionMinutos,
                Disponible = servicio.Disponible,
                Accion = accion
            };

            // CORRECCIÓN APLICADA: Routing Key dinámica (ej: servicio.creado)
            string routingKey = $"servicio.{accion.ToLower()}";

            // Se usa el routingKey dinámico en el PublishAsync
            return _messagePublisher.PublishAsync(evento, routingKey, "servicio_exchange");
        }
        // ------------------------------------------------

        public async Task<ServicioDto> CreateAsync(CreateServicioRequestDto requestDto)
        {
            if (requestDto.Imagen != null)
            {
                ValidateImageFile(requestDto.Imagen.Length, requestDto.Imagen.ContentType);
            }

            var categoriaExistente = await _categoriaRepo.GetByIdAsync(requestDto.CategoriaId);

            if (categoriaExistente == null)
            {
                throw new ArgumentException($"La categoría con ID {requestDto.CategoriaId} no existe.");
            }

            if (!TryConvertPrecio(requestDto.Precio, out double precioValor))
            {
                throw new ArgumentException("El precio debe ser un valor numérico válido mayor o igual a 1. Utiliza el punto como separador decimal (ej: 50000.00).");
            }

            var servicio = new Servicio
            {
                Nombre = requestDto.Nombre,
                Descripcion = requestDto.Descripcion,
                DuracionMinutos = requestDto.DuracionMinutos,
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

            await PublishServicioEvent(servicioCompleto!, "CREADO");

            return MapToDto(servicioCompleto!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // OBTENER el servicio antes de eliminarlo para poder enviar el evento.
            var servicio = await _servicioRepo.GetByIdAsync(id);
            if (servicio == null) return false;

            var success = await _servicioRepo.DeleteAsync(id);

            if (success)
            {
                await PublishServicioEvent(servicio, "ELIMINADO");
            }

            return success;
        }

        public async Task<ServicioDto?> UpdateAsync(int id, UpdateServicioRequestDto requestDto)
        {
            if (requestDto.Imagen != null)
            {
                ValidateImageFile(requestDto.Imagen.Length, requestDto.Imagen.ContentType);
            }

            var categoriaExistente = await _categoriaRepo.GetByIdAsync(requestDto.CategoriaId);

            if (categoriaExistente == null)
            {
                throw new ArgumentException($"La categoría con ID {requestDto.CategoriaId} no existe.");
            }

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
            servicioExistente.DuracionMinutos = requestDto.DuracionMinutos;

            if (requestDto.Imagen != null && requestDto.Imagen.Length > 0)
            {
                var nombreArchivo = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images");
                servicioExistente.Imagen = nombreArchivo;
            }

            var servicioGuardado = await _servicioRepo.UpdateAsync(id, servicioExistente);

            if (servicioGuardado == null)
            {
                return null;
            }

            var servicioCompleto = await _servicioRepo.GetByIdAsync(servicioGuardado.Id);

            await PublishServicioEvent(servicioCompleto!, "ACTUALIZADO");

            if (servicioCompleto == null)
            {
                return MapToDto(servicioGuardado);
            }

            return MapToDto(servicioCompleto);
        }

        // --- (Métodos auxiliares y GetAll/GetById/GetByCategoriaIdAsync no se modifican) ---

        private bool TryConvertPrecio(string? precioString, out double precioValor)
        {
            precioValor = 0;
            if (string.IsNullOrWhiteSpace(precioString)) return false;

            string cleanedString = precioString.Replace(',', '.');

            if (double.TryParse(cleanedString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) && result >= 1)
            {
                precioValor = result;
                return true;
            }

            return false;
        }

        private void ValidateImageFile(long fileSize, string contentType)
        {
            const int maxFileSize = 5 * 1024 * 1024;
            var allowedContentTypes = new[] { "image/jpeg", "image/png" };

            if (fileSize > maxFileSize)
            {
                throw new ArgumentException("El archivo de imagen no puede exceder los 5 MB.");
            }

            if (!allowedContentTypes.Contains(contentType.ToLower()))
            {
                throw new ArgumentException("Formato de archivo no válido. Solo se permiten imágenes JPEG o PNG.");
            }
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

        public async Task<IEnumerable<ServicioDto>> GetAllAsync()
        {
            var servicios = await _servicioRepo.GetAllAsync();
            return servicios.Select(MapToDto);
        }

        public async Task<ServicioDto?> GetByIdAsync(int id)
        {
            var servicio = await _servicioRepo.GetByIdAsync(id);
            return servicio == null ? null : MapToDto(servicio);
        }

        public async Task<IEnumerable<ServicioDto>> GetByCategoriaIdAsync(int categoriaId)
        {
            var servicios = await _servicioRepo.GetByCategoriaIdAsync(categoriaId);
            return servicios.Select(MapToDto);
        }
    }
}