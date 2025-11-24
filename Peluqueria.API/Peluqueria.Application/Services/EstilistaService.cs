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
        private readonly IFileStorageService _fileStorage;

        public EstilistaService(IEstilistaRepository estilistaRepo, IMessagePublisher messagePublisher, IIdentityService identityService, IServicioRepository servicioRepo, IEstilistaAgendaService agendaService, IFileStorageService fileStorage)
        {
            _estilistaRepo = estilistaRepo;
            _messagePublisher = messagePublisher;
            _identityService = identityService;
            _servicioRepo = servicioRepo;
            _agendaService = agendaService;
            _fileStorage = fileStorage;
        }

        public async Task<IEnumerable<EstilistaDto>> GetAllAsync()
        {
            var estilistas = await _estilistaRepo.GetAllAsync();

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
                Imagen = estilista.Imagen,
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
                ImagenUrl = estilista.Imagen,
                Accion = accion,
                ServiciosAsociados = estilista.ServiciosAsociados.Select(es => new EstilistaServicioMinimalEventDto
                {
                    ServicioId = es.ServicioId,
                    DuracionMinutos = es.Servicio.DuracionMinutos
                }).ToList()
            };

            await _messagePublisher.PublishAsync(evento, $"estilista.{accion.ToLower()}", "estilista_exchange");
        }

        public async Task<EstilistaDto> CreateAsync(CreateEstilistaRequestDto requestDto)
        {
            if (requestDto.ServiciosIds.Count == 0)
            {
                throw new ArgumentException("Un estilista debe tener al menos un servicio asociado. (G-ERROR-006)");
            }

            var createIdentity = await _identityService.CreateUserAsync(
                requestDto.Username, 
                requestDto.Email, 
                requestDto.Password, 
                requestDto.NombreCompleto, 
                requestDto.Telefono
            );

            if (!createIdentity.Succeeded)
            {
                throw new Exception("Fallo en la creación de credenciales: " + string.Join(", ", createIdentity.Errors.Select(e => e.Description)));
            }

            var roleResult = await _identityService.AddUserToRoleAsync(requestDto.Username, "Estilista");
            if (!roleResult.Succeeded)
            {
                throw new Exception("Fallo al asignar el rol Estilista.");
            }

            var userDetails = await _identityService.FindByNameAsync(requestDto.Username);
            if (userDetails == null)
            {
                throw new Exception("Error interno: No se pudo encontrar el usuario después de la creación.");
            }

            string? imageName = null;
            if (requestDto.Imagen != null)
            {
                imageName = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images/estilistas");
            }

            var estilista = new Estilista
            {
                IdentityId = userDetails.Id,
                NombreCompleto = requestDto.NombreCompleto,
                Telefono = requestDto.Telefono,
                EstaActivo = true,
                Imagen = imageName ?? string.Empty
            };
            var nuevoEstilista = await _estilistaRepo.CreateAsync(estilista, requestDto.ServiciosIds);
            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(nuevoEstilista.Id);

            await PublishEstilistaEventAsync(estilistaCompleto!, "CREADO");

            var estilistaDto = MapToDto(estilistaCompleto!);
            estilistaDto.Email = userDetails.Email;
            return estilistaDto;
        }

        public async Task<bool> InactivateAsync(int id)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilista == null) return false;

            estilista.EstaActivo = false;
            var updated = await _estilistaRepo.UpdateAsync(estilista, estilista.ServiciosAsociados.Select(es => es.ServicioId).ToList());

            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilistaCompleto != null)
            {
                await PublishEstilistaEventAsync(estilistaCompleto, "INACTIVADO");
            }

            return true;
        }

        public async Task<EstilistaDto?> UpdateAsync(int id, UpdateEstilistaRequestDto requestDto)
        {
            if (requestDto.ServiciosIds.Count == 0) throw new ArgumentException("G-ERROR-006");

            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilista == null) return null;

            if (!string.IsNullOrWhiteSpace(requestDto.Username) || !string.IsNullOrWhiteSpace(requestDto.Email))
            {

                var identityResult = await _identityService.UpdateUserCredentialsAsync(
                    estilista.IdentityId,
                    requestDto.Username ?? "",
                    requestDto.Email ?? ""
                );

                if (!identityResult.Succeeded)
                {
                    var errores = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    throw new Exception($"Error al actualizar credenciales: {errores}");
                }
            }

            if (!string.IsNullOrWhiteSpace(requestDto.Password))
            {
                var passResult = await _identityService.AdminResetPasswordAsync(estilista.IdentityId, requestDto.Password);
                if (!passResult.Succeeded) throw new Exception("Error al actualizar contraseña.");
            }

            string? imageName = estilista.Imagen;
            if (requestDto.Imagen != null)
            {
                imageName = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images/estilistas");
            }

            var estilistaToUpdate = new Estilista
            {
                Id = id,
                IdentityId = estilista.IdentityId,
                NombreCompleto = requestDto.NombreCompleto,
                Telefono = requestDto.Telefono,
                EstaActivo = estilista.EstaActivo,
                Imagen = imageName ?? string.Empty
            };

            var updatedResult = await _estilistaRepo.UpdateAsync(estilistaToUpdate, requestDto.ServiciosIds);

            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            await PublishEstilistaEventAsync(estilistaCompleto!, "ACTUALIZADO");

            var userDetails = await _identityService.FindByIdentityIdAsync(estilistaCompleto!.IdentityId);
            var dto = MapToDto(estilistaCompleto);
            dto.Email = userDetails?.Email ?? string.Empty;

            return dto;
        }
    }
}