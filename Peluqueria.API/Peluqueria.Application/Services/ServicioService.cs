using Peluqueria.Application.Dtos.Servicio;
using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Exceptions;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using System.Globalization;

namespace Peluqueria.Application.Services
{
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _servicioRepo;
        private readonly IFileStorageService _fileStorage;
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IMessagePublisher _messagePublisher;
        // Inyectamos el cliente HTTP para validaciones
        private readonly IReservacionClient _reservacionClient;

        public ServicioService(
            IServicioRepository servicioRepo,
            IFileStorageService fileStorage,
            ICategoriaRepository categoriaRepo,
            IMessagePublisher messagePublisher,
            IReservacionClient reservacionClient) // <--- INYECCIÓN
        {
            _servicioRepo = servicioRepo;
            _fileStorage = fileStorage;
            _categoriaRepo = categoriaRepo;
            _messagePublisher = messagePublisher;
            _reservacionClient = reservacionClient;
        }

        public async Task<ServicioDto> CreateAsync(CreateServicioRequestDto requestDto)
        {
            // 1. VALIDAR DURACIÓN (Regla de Negocio RNI-S001 y Política Operativa)
            ValidateDuracion(requestDto.DuracionMinutos);

            // 2. VALIDACIÓN IMAGEN (G-ERROR-013)
            if (requestDto.Imagen != null)
            {
                ValidateImageFile(requestDto.Imagen.Length, requestDto.Imagen.ContentType);
            }

            // 3. VALIDACIÓN CATEGORÍA EXISTENTE (G-ERROR-012)
            var categoriaExistente = await _categoriaRepo.GetByIdAsync(requestDto.CategoriaId);
            if (categoriaExistente == null)
            {
                throw new EntidadNoExisteException(CodigoError.CATEGORIA_NO_ENCONTRADA);
            }

            // 4. VALIDACIÓN NOMBRE DUPLICADO (G-ERROR-010)
            if (await _servicioRepo.ExistsByNameAsync(requestDto.Nombre))
            {
                throw new EntidadYaExisteException(CodigoError.SERVICIO_NOMBRE_DUPLICADO);
            }

            // 5. VALIDACIÓN PRECIO (G-ERROR-003)
            if (!TryConvertPrecio(requestDto.Precio, out double precioValor))
            {
                throw new ReglaNegocioException(CodigoError.PRECIO_INVALIDO);
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

            await PublishServicioEvent(nuevoServicio, "CREADO");

            var servicioCompleto = await _servicioRepo.GetByIdAsync(nuevoServicio.Id);
            return MapToDto(servicioCompleto!);
        }

        public async Task<ServicioDto?> UpdateAsync(int id, UpdateServicioRequestDto requestDto)
        {
            // 1. VALIDACIÓN EXISTENCIA (G-ERROR-011)
            var servicioExistente = await _servicioRepo.GetByIdAsync(id);
            if (servicioExistente == null)
            {
                throw new EntidadNoExisteException(CodigoError.SERVICIO_NO_ENCONTRADO);
            }

            // 2. VALIDACIÓN BLOQUEO POR CITAS (G-ERROR-004)
            // Consultamos al microservicio si existen reservas futuras para este servicio
            bool tieneCitasFuturas = await _reservacionClient.TieneReservasServicio(id);

            if (tieneCitasFuturas)
            {
                throw new ReglaNegocioException(CodigoError.SERVICIO_BLOQUEADO_POR_CITAS);
            }

            // 3. VALIDAR DURACIÓN (Regla de Negocio)
            ValidateDuracion(requestDto.DuracionMinutos);

            // 4. VALIDACIÓN CATEGORÍA (G-ERROR-012)
            var categoriaExistente = await _categoriaRepo.GetByIdAsync(requestDto.CategoriaId);
            if (categoriaExistente == null)
            {
                throw new EntidadNoExisteException(CodigoError.CATEGORIA_NO_ENCONTRADA);
            }

            // 5. VALIDACIÓN IMAGEN (G-ERROR-013)
            if (requestDto.Imagen != null)
            {
                ValidateImageFile(requestDto.Imagen.Length, requestDto.Imagen.ContentType);
            }

            // 6. VALIDACIÓN PRECIO (G-ERROR-003)
            if (!TryConvertPrecio(requestDto.Precio, out double precioValor))
            {
                throw new ReglaNegocioException(CodigoError.PRECIO_INVALIDO);
            }

            // Actualización
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

            await PublishServicioEvent(servicioGuardado!, "ACTUALIZADO");

            return MapToDto(servicioGuardado!);
        }

        public async Task<bool> InactivateAsync(int id)
        {
            var servicio = await _servicioRepo.GetByIdAsync(id);
            if (servicio == null)
            {
                throw new EntidadNoExisteException(CodigoError.SERVICIO_NO_ENCONTRADO);
            }

            bool tieneCitasFuturas = await _reservacionClient.TieneReservasServicio(id);

            if (tieneCitasFuturas)
            {
                throw new ReglaNegocioException(CodigoError.SERVICIO_BLOQUEADO_POR_CITAS);
            }
            // ------------------------------------

            var success = await _servicioRepo.InactivateAsync(id);

            if (success)
            {
                servicio.Disponible = false;
                await PublishServicioEvent(servicio, "INACTIVADO");
            }

            return success;
        }

        // --- MÉTODOS PRIVADOS ---

        // Validar Regla de Negocio de Duración
        private void ValidateDuracion(int minutos)
        {
            // RNI-S001: Mínimo 45 minutos
            if (minutos < 45)
            {
                // Usamos el constructor que acepta mensaje personalizado
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "La duración mínima permitida es de 45 minutos.");
            }

            // Política Operativa: Máximo 8 horas (480 min)
            if (minutos > 480)
            {
                throw new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "La duración no puede exceder las 8 horas (480 minutos).");
            }
        }

        private bool TryConvertPrecio(string? precioString, out double precioValor)
        {
            precioValor = 0;
            if (string.IsNullOrWhiteSpace(precioString)) return false;

            string cleanedString = precioString.Replace(',', '.');

            if (double.TryParse(cleanedString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) && result > 0)
            {
                precioValor = result;
                return true;
            }
            return false;
        }

        private void ValidateImageFile(long fileSize, string contentType)
        {
            const int maxFileSize = 5 * 1024 * 1024; // 5MB
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/jpg" };

            if (fileSize > maxFileSize || !allowedContentTypes.Contains(contentType.ToLower()))
            {
                throw new ReglaNegocioException(CodigoError.IMAGEN_INVALIDA);
            }
        }

        public async Task<IEnumerable<ServicioDto>> GetAllAsync()
        {
            var servicios = await _servicioRepo.GetAllAsync();
            return servicios.Select(MapToDto);
        }

        public async Task<ServicioDto?> GetByIdAsync(int id)
        {
            var servicio = await _servicioRepo.GetByIdAsync(id);

            if (servicio == null)
            {
                throw new EntidadNoExisteException(CodigoError.SERVICIO_NO_ENCONTRADO);
            }

            return MapToDto(servicio);
        }

        public async Task<IEnumerable<ServicioDto>> GetByCategoriaIdAsync(int categoriaId)
        {
            var categoria = await _categoriaRepo.GetByIdAsync(categoriaId);

            if (categoria == null)
            {
                throw new EntidadNoExisteException(CodigoError.CATEGORIA_NO_ENCONTRADA);
            }

            var servicios = await _servicioRepo.GetByCategoriaIdAsync(categoriaId);

            return servicios.Select(MapToDto);
        }

        private Task PublishServicioEvent(Servicio servicio, string accion)
        {
            var evento = new ServicioEventDto
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                DuracionMinutos = servicio.DuracionMinutos,
                Precio = servicio.Precio,
                CategoriaId = servicio.CategoriaId,
                Disponible = servicio.Disponible,
                Accion = accion
            };
            string routingKey = $"servicio.{accion.ToLower()}";
            return _messagePublisher.PublishAsync(evento, routingKey, "servicio_exchange");
        }

        private static ServicioDto MapToDto(Servicio servicio)
        {
            return new ServicioDto
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                Descripcion = servicio.Descripcion,
                DuracionMinutos = servicio.DuracionMinutos,
                Precio = servicio.Precio,
                Imagen = servicio.Imagen,
                FechaCreacion = servicio.FechaCreacion,
                Disponible = servicio.Disponible,
                Categoria = servicio.Categoria != null ? new CategoriaDto
                {
                    Id = servicio.Categoria.Id,
                    Nombre = servicio.Categoria.Nombre,
                    EstaActiva = servicio.Categoria.EstaActiva
                } : null!
            };
        }
    }
}