using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Interfaces;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Services
{
    public class EstilistaService : IEstilistaService
    {
        private readonly IEstilistaRepository _estilistaRepo;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IIdentityService _identityService;
        private readonly IServicioRepository _servicioRepo;
        private readonly IEstilistaAgendaService _agendaService;

        public EstilistaService(IEstilistaRepository estilistaRepo, IMessagePublisher messagePublisher, IIdentityService identityService, IServicioRepository servicioRepo, IEstilistaAgendaService agendaService)
        {
            _estilistaRepo = estilistaRepo;
            _messagePublisher = messagePublisher;
            _identityService = identityService;
            _servicioRepo = servicioRepo;
            _agendaService = agendaService;
        }

        public async Task<IEnumerable<EstilistaDto>> GetAllAsync()
        {
            var estilistas = await _estilistaRepo.GetAllAsync();

            // Necesitamos obtener el email de Identity para cada uno
            var dtos = new List<EstilistaDto>();

            foreach (var estilista in estilistas)
            {
                var userDetails = await _identityService.FindByIdentityIdAsync(estilista.IdentityId);
                var dto = MapToDto(estilista);
                dto.Email = userDetails?.Email ?? string.Empty;
                dtos.Add(dto);
            }

            return dtos;
        }

        // Implementación de la consulta por ID
        public async Task<EstilistaDto?> GetByIdAsync(int id)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilista == null) return null;

            var userDetails = await _identityService.FindByIdentityIdAsync(estilista.IdentityId);
            var dto = MapToDto(estilista);
            dto.Email = userDetails?.Email ?? string.Empty;

            return dto;
        }

        private EstilistaDto MapToDto(Estilista estilista)
        {
            return new EstilistaDto
            {
                Id = estilista.Id,
                NombreCompleto = estilista.NombreCompleto,
                Telefono = estilista.Telefono,
                EstaActivo = estilista.EstaActivo,
                ServiciosIds = estilista.ServiciosAsociados.Select(es => es.ServicioId).ToList()

            };
        }

        private async Task PublishEstilistaEventAsync(Estilista estilista, string accion)
        {
            var userDetails = await _identityService.FindByIdentityIdAsync(estilista.IdentityId);

            var evento = new EstilistaEventDto
            {
                Id = estilista.Id,
                IdentityId = estilista.IdentityId,
                NombreCompleto = estilista.NombreCompleto,
                Email = userDetails?.Email ?? string.Empty,
                Telefono = estilista.Telefono,
                EstaActivo = estilista.EstaActivo,
                Accion = accion,
                ServiciosAsociados = estilista.ServiciosAsociados.Select(es => new EstilistaServicioMinimalEventDto
                {
                    ServicioId = es.ServicioId,
                    // La propiedad Servicio debe ser cargada en el repositorio (GetFullEstilistaByIdAsync)
                    DuracionMinutos = es.Servicio.DuracionMinutos
                }).ToList()
            };

            await _messagePublisher.PublishAsync(evento, $"estilista.{accion.ToLower()}", "estilista_exchange");
        }

        public async Task<EstilistaDto> CreateAsync(CreateEstilistaRequestDto requestDto)
        {
            // 1. VALIDACIÓN (Servicios Mínimos - RNI-E003)
            if (requestDto.ServiciosIds.Count == 0)
            {
                throw new ArgumentException("Un estilista debe tener al menos un servicio asociado. (G-ERROR-006)");
            }

            // 2. CREACIÓN DE USUARIO IDENTITY (RNI-E002: Unicidad Credencial)
            var createIdentity = await _identityService.CreateUserAsync(requestDto.Username, requestDto.Email, requestDto.Password, requestDto.NombreCompleto, requestDto.Telefono);
            if (!createIdentity.Succeeded)
            {
                throw new Exception("Fallo en la creación de credenciales: " + string.Join(", ", createIdentity.Errors.Select(e => e.Description)));
            }

            // 3. ASIGNACIÓN DE ROL
            var roleResult = await _identityService.AddUserToRoleAsync(requestDto.Username, "Estilista");
            if (!roleResult.Succeeded)
            {
                throw new Exception("Fallo al asignar el rol Estilista.");
            }

            // 4. CREACIÓN DE ESTILISTA (Perfil de Dominio)
            var userDetails = await _identityService.FindByNameAsync(requestDto.Username);
            if (userDetails == null)
            {
                throw new Exception("Error interno: No se pudo encontrar el usuario después de la creación.");
            }

            var estilista = new Estilista
            {
                IdentityId = userDetails.Id,
                NombreCompleto = requestDto.NombreCompleto,
                Telefono = requestDto.Telefono,
                EstaActivo = true
            };
            var nuevoEstilista = await _estilistaRepo.CreateAsync(estilista, requestDto.ServiciosIds);
            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(nuevoEstilista.Id);

            // 5. PUBLICACIÓN DE EVENTO (Notificar al MR)
            await PublishEstilistaEventAsync(estilistaCompleto!, "CREADO");

            // CORRECCIÓN: Llamar al método MapToDto e inyectar el email obtenido de Identity.
            var estilistaDto = MapToDto(estilistaCompleto!);
            estilistaDto.Email = userDetails.Email; // Llenar el campo Email del DTO desde Identity
            return estilistaDto;
        }

        public async Task<bool> InactivateAsync(int id)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilista == null) return false;

            // TODO: VALIDACIÓN CRÍTICA (RN-BLOQUEO-CITAS / G-ERROR-009)

            // Baja Lógica (Monolito SoT)
            estilista.EstaActivo = false;
            // Al llamar a UpdateAsync, enviamos la nueva lista de servicios (la misma que ya tenía)
            var updated = await _estilistaRepo.UpdateAsync(estilista, estilista.ServiciosAsociados.Select(es => es.ServicioId).ToList());

            // Publicación de Evento (Notificar al MR)
            // Se debe recargar el estilista completo para asegurar los Servicios.DuracionMinutos en el evento.
            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilistaCompleto != null)
            {
                await PublishEstilistaEventAsync(estilistaCompleto, "INACTIVADO");
            }


            return true;
        }

        public async Task<EstilistaDto?> UpdateAsync(int id, UpdateEstilistaRequestDto requestDto)
        {
            // 1. VALIDACIÓN (Servicios Mínimos - RNI-E003)
            if (requestDto.ServiciosIds.Count == 0)
            {
                throw new ArgumentException("Un estilista debe tener al menos un servicio asociado. (G-ERROR-006)");
            }

            // TODO: VALIDACIÓN CRÍTICA (Bloqueo por Citas Futuras - G-ERROR-004/G-ERROR-009)
            // Esto debería hacerse en el controlador o en un servicio de validación de MR.

            // 2. Encontrar estilista existente
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilista == null) return null;

            // 3. Crear el objeto Estilista con los datos actualizados
            var estilistaToUpdate = new Estilista
            {
                Id = id,
                // RNI-S005. Restricción de edición: Se mantiene el IdentityId.
                IdentityId = estilista.IdentityId,
                NombreCompleto = requestDto.NombreCompleto,
                Telefono = requestDto.Telefono,
                // El estado de actividad se mantiene igual a menos que sea la acción InactivateAsync
                EstaActivo = estilista.EstaActivo
            };

            // 4. Actualizar en el repositorio (maneja M:N de Servicios)
            var updatedEstilistaMinimal = await _estilistaRepo.UpdateAsync(estilistaToUpdate, requestDto.ServiciosIds);
            if (updatedEstilistaMinimal == null) return null;

            // 5. Cargar la entidad completa para el DTO y el Evento (para obtener DuracionMinutos de los servicios)
            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilistaCompleto == null)
            {
                // Debería ser unreachable si updatedEstilistaMinimal no es null
                throw new Exception("Error interno: No se pudo cargar el estilista después de la actualización.");
            }

            // 6. PUBLICACIÓN DE EVENTO (Notificar al MR)
            await PublishEstilistaEventAsync(estilistaCompleto, "ACTUALIZADO");

            // 7. Mapear DTO (necesita el email de Identity)
            var userDetails = await _identityService.FindByIdentityIdAsync(estilistaCompleto.IdentityId);
            var estilistaDto = MapToDto(estilistaCompleto);
            estilistaDto.Email = userDetails?.Email ?? string.Empty; // Llenar el campo Email del DTO desde Identity

            return estilistaDto;
        }
    }
}