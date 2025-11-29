using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Exceptions;
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
        private readonly IReservacionClient _reservacionClient;

        public EstilistaService(IEstilistaRepository estilistaRepo,
                                IMessagePublisher messagePublisher,
                                IIdentityService identityService,
                                IServicioRepository servicioRepo,
                                IEstilistaAgendaService agendaService,
                                IFileStorageService fileStorage,
                                IReservacionClient reservacionClient)
        {
            _estilistaRepo = estilistaRepo;
            _messagePublisher = messagePublisher;
            _identityService = identityService;
            _servicioRepo = servicioRepo;
            _agendaService = agendaService;
            _fileStorage = fileStorage;
            _reservacionClient = reservacionClient;
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

        public async Task<EstilistaDto> GetByIdAsync(int id)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);

            if (estilista == null)
            {
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);
            }

            var userDetails = await _identityService.FindByIdentityIdAsync(estilista.IdentityId);
            var dto = MapToDto(estilista);
            dto.Email = userDetails?.Email ?? string.Empty;

            return dto;
        }

        public async Task<EstilistaDto> CreateAsync(CreateEstilistaRequestDto requestDto)
        {
            if (requestDto.ServiciosIds == null || requestDto.ServiciosIds.Count == 0)
            {
                throw new ReglaNegocioException(CodigoError.ESTILISTA_SIN_SERVICIOS);
            }

            requestDto.ServiciosIds = requestDto.ServiciosIds.Distinct().ToList();

            foreach (var servicioId in requestDto.ServiciosIds)
            {
                var servicio = await _servicioRepo.GetByIdAsync(servicioId);
                if (servicio == null)
                {
                    throw new EntidadNoExisteException(CodigoError.SERVICIO_NO_ENCONTRADO, $"El servicio con ID {servicioId} no existe.");
                }
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
                if (createIdentity.Errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail"))
                {
                    throw new EntidadYaExisteException(CodigoError.ENTIDAD_YA_EXISTE);
                }

                var errorMsg = string.Join("; ", createIdentity.Errors.Select(e => e.Description));
                throw new ReglaNegocioException(CodigoError.ERROR_GENERICO, $"Fallo creando usuario: {errorMsg}");
            }

            var roleResult = await _identityService.AddUserToRoleAsync(requestDto.Username, "Estilista");
            if (!roleResult.Succeeded)
            {
                throw new ReglaNegocioException(CodigoError.ERROR_GENERICO, "Fallo al asignar rol de Estilista.");
            }

            var userDetails = await _identityService.FindByNameAsync(requestDto.Username);

            string? imageName = null;
            if (requestDto.Imagen != null)
            {
                imageName = await _fileStorage.SaveFileAsync(requestDto.Imagen, "images/estilistas");
            }

            var estilista = new Estilista
            {
                IdentityId = userDetails!.Id,
                NombreCompleto = requestDto.NombreCompleto,
                Telefono = requestDto.Telefono,
                EstaActivo = true,
                Imagen = imageName ?? string.Empty
            };

            var nuevoEstilista = await _estilistaRepo.CreateAsync(estilista, requestDto.ServiciosIds);
            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(nuevoEstilista.Id);

            await PublishEstilistaEventAsync(estilistaCompleto!, "CREADO");

            var estilistaDto = MapToDto(estilistaCompleto!);
            estilistaDto.Email = userDetails.Email!;
            return estilistaDto;
        }

        public async Task<EstilistaDto?> UpdateAsync(int id, UpdateEstilistaRequestDto requestDto)
        {
            if (requestDto.ServiciosIds.Count == 0)
                throw new ReglaNegocioException(CodigoError.ESTILISTA_SIN_SERVICIOS);

            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilista == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            requestDto.ServiciosIds = requestDto.ServiciosIds.Distinct().ToList();

            foreach (var servicioId in requestDto.ServiciosIds)
            {
                var servicio = await _servicioRepo.GetByIdAsync(servicioId);
                if (servicio == null)
                {
                    throw new EntidadNoExisteException(CodigoError.SERVICIO_NO_ENCONTRADO, $"El servicio con ID {servicioId} no existe.");
                }
            }

            if (!string.IsNullOrWhiteSpace(requestDto.Username) || !string.IsNullOrWhiteSpace(requestDto.Email))
            {
                var identityResult = await _identityService.UpdateUserCredentialsAsync(
                    estilista.IdentityId,
                    requestDto.Username ?? "",
                    requestDto.Email ?? ""
                );

                if (!identityResult.Succeeded)
                {
                    if (identityResult.Errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail"))
                        throw new EntidadYaExisteException(CodigoError.ENTIDAD_YA_EXISTE);

                    var errorMsg = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                    throw new ReglaNegocioException(CodigoError.ERROR_GENERICO, $"Error credenciales: {errorMsg}");
                }
            }

            if (!string.IsNullOrWhiteSpace(requestDto.Password))
            {
                var passResult = await _identityService.AdminResetPasswordAsync(estilista.IdentityId, requestDto.Password);

                if (!passResult.Succeeded)
                {
                    var errorMsg = string.Join("; ", passResult.Errors.Select(e => e.Description));
                    throw new ReglaNegocioException(CodigoError.SEGURIDAD_CUENTA, $"La contraseña no es válida: {errorMsg}");
                }
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

            await _estilistaRepo.UpdateAsync(estilistaToUpdate, requestDto.ServiciosIds);

            var estilistaCompleto = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            await PublishEstilistaEventAsync(estilistaCompleto!, "ACTUALIZADO");

            var userDetails = await _identityService.FindByIdentityIdAsync(estilistaCompleto!.IdentityId);
            var dto = MapToDto(estilistaCompleto);
            dto.Email = userDetails?.Email ?? string.Empty;

            return dto;
        }

        public async Task<bool> InactivateAsync(int id)
        {
            var estilista = await _estilistaRepo.GetFullEstilistaByIdAsync(id);
            if (estilista == null)
                throw new EntidadNoExisteException(CodigoError.ENTIDAD_NO_ENCONTRADA);

            // --- VALIDACIÓN CON MICROSERVICIO ---
            // Consultamos al microservicio si existen reservas futuras
            bool tieneCitas = await _reservacionClient.TieneReservasEstilista(id);

            if (tieneCitas)
            {
                // Si tiene citas, lanzamos la excepción con tu código de error G-ERROR-009
                throw new ReglaNegocioException(CodigoError.OPERACION_BLOQUEADA_POR_CITAS,
                    "El estilista tiene reservaciones futuras pendientes y no puede ser inactivado.");
            }
            // ------------------------------------

            estilista.EstaActivo = false;
            var serviciosIds = estilista.ServiciosAsociados.Select(s => s.ServicioId).ToList();

            await _estilistaRepo.UpdateAsync(estilista, serviciosIds);

            await PublishEstilistaEventAsync(estilista, "INACTIVADO");

            return true;
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
    }
}