using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;
using Peluqueria.Application.Dtos.Events;

namespace Peluqueria.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IMessagePublisher _messagePublisher; // <-- AÑADIDO

        // Constructor con inyección de IMessagePublisher
        public CategoriaService(ICategoriaRepository categoriaRepo, IMessagePublisher messagePublisher)
        {
            _categoriaRepo = categoriaRepo;
            _messagePublisher = messagePublisher;
        }

        // --- Método Auxiliar de Eventos ---
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
        // -----------------------------------

        public async Task<IEnumerable<CategoriaDto>> GetAllAsync()
        {
            var categorias = await _categoriaRepo.GetAllAsync();

            var categoriaDtos = categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre
            });

            return categoriaDtos;
        }

        // PEL-HU-21: Crear
        public async Task<CategoriaDto> CreateAsync(CreateCategoriaRequestDto requestDto)
        {
            // RN-Categoría-Existe: Validar unicidad (G-ERROR-007)
            var existing = await _categoriaRepo.GetByNameAsync(requestDto.Nombre);
            if (existing != null)
            {
                throw new ArgumentException("Categoría ya existe. (G-ERROR-007)");
            }

            var categoria = new Categoria
            {
                Nombre = requestDto.Nombre,
                EstaActiva = true // Nueva categoría activa por defecto
            };

            var newCategoria = await _categoriaRepo.CreateAsync(categoria);

            // Publicar Evento
            await PublishCategoriaEventAsync(newCategoria, "CREADA");

            return new CategoriaDto { Id = newCategoria.Id, Nombre = newCategoria.Nombre };
        }

        // PEL-HU-22: Actualizar
        public async Task<CategoriaDto?> UpdateAsync(int id, UpdateCategoriaRequestDto requestDto)
        {
            var existingCategoria = await _categoriaRepo.GetByIdAsync(id);
            if (existingCategoria == null) return null;

            // RN-Categoría-Existe: Validar si el nuevo nombre ya existe en OTRA categoría (G-ERROR-007)
            var duplicate = await _categoriaRepo.GetByNameAsync(requestDto.Nombre);
            if (duplicate != null && duplicate.Id != id)
            {
                throw new ArgumentException("Categoría ya existe. (G-ERROR-007)");
            }

            var categoriaToUpdate = new Categoria
            {
                Id = id,
                Nombre = requestDto.Nombre,
                EstaActiva = requestDto.EstaActiva // Permitir reactivación
            };

            var updatedCategoria = await _categoriaRepo.UpdateAsync(id, categoriaToUpdate);
            if (updatedCategoria == null) return null;

            // Publicar Evento
            await PublishCategoriaEventAsync(updatedCategoria, "ACTUALIZADA");

            return new CategoriaDto { Id = updatedCategoria.Id, Nombre = updatedCategoria.Nombre };
        }

        // PEL-HU-23: Inactivar
        public async Task<bool> InactivateAsync(int id)
        {
            var existingCategoria = await _categoriaRepo.GetByIdAsync(id);
            if (existingCategoria == null) return false;

            // TODO: RN-Categoría-Servicios: Validar si existen servicios asociados (G-ERROR-008)
            // Esto implica añadir un método a IServicioRepository para contar servicios por CategoriaId.
            // Por ahora, asumimos que la validación se hace.

            var success = await _categoriaRepo.InactivateAsync(id);

            if (success)
            {
                // Publicar Evento
                existingCategoria.EstaActiva = false; // Estado para el evento
                await PublishCategoriaEventAsync(existingCategoria, "INACTIVADA");
            }

            return success;
        }
    }
}