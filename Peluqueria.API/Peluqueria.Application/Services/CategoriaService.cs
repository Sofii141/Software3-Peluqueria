using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Exceptions;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IServicioRepository _servicioRepo;
        private readonly IMessagePublisher _messagePublisher;
        // Inyectamos el cliente para validación externa
        private readonly IReservacionClient _reservacionClient;

        public CategoriaService(ICategoriaRepository categoriaRepo,
                                IServicioRepository servicioRepo,
                                IMessagePublisher messagePublisher,
                                IReservacionClient reservacionClient) // <--- INYECCIÓN
        {
            _categoriaRepo = categoriaRepo;
            _servicioRepo = servicioRepo;
            _messagePublisher = messagePublisher;
            _reservacionClient = reservacionClient;
        }

        public async Task<IEnumerable<CategoriaDto>> GetAllAsync()
        {
            var categorias = await _categoriaRepo.GetAllAsync();
            return categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                EstaActiva = c.EstaActiva
            });
        }

        public async Task<CategoriaDto> CreateAsync(CreateCategoriaRequestDto requestDto)
        {
            var existing = await _categoriaRepo.GetByNameAsync(requestDto.Nombre);
            if (existing != null)
            {
                throw new EntidadYaExisteException(CodigoError.CATEGORIA_YA_EXISTE);
            }

            var categoria = new Categoria
            {
                Nombre = requestDto.Nombre,
                EstaActiva = true
            };

            var newCategoria = await _categoriaRepo.CreateAsync(categoria);

            await PublishCategoriaEventAsync(newCategoria, "CREADA");

            return new CategoriaDto { Id = newCategoria.Id, Nombre = newCategoria.Nombre, EstaActiva = newCategoria.EstaActiva };
        }

        public async Task<CategoriaDto?> UpdateAsync(int id, UpdateCategoriaRequestDto requestDto)
        {
            var existingCategoria = await _categoriaRepo.GetByIdAsync(id);
            if (existingCategoria == null)
            {
                throw new EntidadNoExisteException(CodigoError.CATEGORIA_NO_ENCONTRADA);
            }

            var duplicate = await _categoriaRepo.GetByNameAsync(requestDto.Nombre);
            if (duplicate != null && duplicate.Id != id)
            {
                throw new EntidadYaExisteException(CodigoError.CATEGORIA_YA_EXISTE);
            }

            existingCategoria.Nombre = requestDto.Nombre;
            existingCategoria.EstaActiva = requestDto.EstaActiva;

            var updatedCategoria = await _categoriaRepo.UpdateAsync(id, existingCategoria);

            await PublishCategoriaEventAsync(updatedCategoria!, "ACTUALIZADA");

            return new CategoriaDto { Id = updatedCategoria!.Id, Nombre = updatedCategoria.Nombre, EstaActiva = updatedCategoria.EstaActiva };
        }

        public async Task<bool> InactivateAsync(int id)
        {
            var existingCategoria = await _categoriaRepo.GetByIdAsync(id);
            if (existingCategoria == null)
            {
                throw new EntidadNoExisteException(CodigoError.CATEGORIA_NO_ENCONTRADA);
            }

            // --- VALIDACIÓN CON MICROSERVICIO ---
            // Consultamos si existen reservas futuras que involucren servicios de esta categoría
            bool tieneReservasAsociadas = await _reservacionClient.TieneReservasCategoria(id);

            if (tieneReservasAsociadas)
            {
                // Usamos G-ERROR-008: Categoría con Servicios (y por ende reservas)
                throw new ReglaNegocioException(CodigoError.CATEGORIA_CON_SERVICIOS,
                    "No se puede inactivar la categoría porque tiene servicios con reservaciones futuras activas.");
            }
            // ------------------------------------

            var success = await _categoriaRepo.InactivateAsync(id);

            if (success)
            {
                existingCategoria.EstaActiva = false;
                await PublishCategoriaEventAsync(existingCategoria, "INACTIVADA");
            }

            return success;
        }

        private Task PublishCategoriaEventAsync(Categoria categoria, string accion)
        {
            var evento = new CategoriaEventDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                EstaActiva = categoria.EstaActiva,
                Accion = accion
            };
            string routingKey = $"categoria.{accion.ToLower()}";
            return _messagePublisher.PublishAsync(evento, routingKey, "categoria_exchange");
        }
    }
}