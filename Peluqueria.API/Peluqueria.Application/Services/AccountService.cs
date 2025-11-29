using Microsoft.AspNetCore.Identity;
using Peluqueria.Application.Dtos.Account;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Exceptions;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IMessagePublisher _messagePublisher;

        public AccountService(IIdentityService identityService, ITokenService tokenService, IMessagePublisher messagePublisher)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _messagePublisher = messagePublisher;
        }

        public async Task<NewUserDto?> LoginAsync(LoginDto loginDto)
        {
            // 1. Buscar usuario
            var userMinimal = await _identityService.GetUserByUsernameAsync(loginDto.Username.ToLower());

            if (userMinimal == null)
            {
                throw new ReglaNegocioException(CodigoError.CREDENCIALES_INVALIDAS, "Credenciales inválidas.");
            }

            var result = await _identityService.CheckPasswordSignInAsync(userMinimal.UserName, loginDto.Password, false);

            if (!result.Succeeded)
            {
                throw new ReglaNegocioException(CodigoError.CREDENCIALES_INVALIDAS, "Credenciales inválidas.");
            }

            var roles = await _identityService.GetRolesAsync(userMinimal.UserName);

            return new NewUserDto
            {
                UserName = userMinimal.UserName!,
                Email = userMinimal.Email!,
                Token = _tokenService.CreateToken(userMinimal, roles)
            };
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
        {
            var createdUser = await _identityService.CreateUserAsync(
                registerDto.Username!,
                registerDto.Email!,
                registerDto.Password!,
                registerDto.NombreCompleto!,
                registerDto.Telefono!
            );

            if (!createdUser.Succeeded)
            {
                if (createdUser.Errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail"))
                {
                    throw new EntidadYaExisteException(CodigoError.ENTIDAD_YA_EXISTE);
                }

                var errorMsg = string.Join("; ", createdUser.Errors.Select(e => e.Description));
                throw new ReglaNegocioException(CodigoError.ERROR_GENERICO, errorMsg);
            }

            var roleResult = await _identityService.AddUserToRoleAsync(registerDto.Username!, "Cliente");

            if (roleResult.Succeeded)
            {
                var userDetails = await _identityService.FindByNameAsync(registerDto.Username!);

                var clienteEvent = new ClienteRegistradoEventDto
                {
                    IdentityId = userDetails.Id,
                    Username = userDetails.UserName,
                    NombreCompleto = registerDto.NombreCompleto!,
                    Email = userDetails.Email,
                    Telefono = registerDto.Telefono!
                };

                await _messagePublisher.PublishAsync(clienteEvent, "cliente.registrado", "cliente_exchange");

                return IdentityResult.Success;
            }
            else
            {
                throw new ReglaNegocioException(CodigoError.ERROR_GENERICO, "No se pudo asignar el rol al usuario.");
            }
        }

        public async Task<NewUserDto?> GetNewUserDto(string username, string password)
        {
            var userMinimal = await _identityService.FindByNameAsync(username);
            if (userMinimal == null) return null;

            var roles = await _identityService.GetRolesAsync(userMinimal.UserName);

            return new NewUserDto
            {
                UserName = userMinimal.UserName!,
                Email = userMinimal.Email!,
                Token = _tokenService.CreateToken(userMinimal, roles)
            };
        }

        public async Task<IEnumerable<ClienteResponseDto>> GetAllClientesAsync()
        {
            // Llamamos a IdentityService pidiendo explícitamente el rol "Cliente"
            return await _identityService.GetUsersByRoleAsync("Cliente");
        }
    }
}