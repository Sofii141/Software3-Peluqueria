using Microsoft.AspNetCore.Identity;
using Peluqueria.Application.Dtos.Account;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Events;

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
            var userMinimal = await _identityService.GetUserByUsernameAsync(loginDto.Username.ToLower());

            if (userMinimal == null)
            {
                return null;
            }

            var result = await _identityService.CheckPasswordSignInAsync(userMinimal.UserName, loginDto.Password, false);

            if (!result.Succeeded)
            {
                return null;
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

            if (createdUser.Succeeded)
            {
                var roleResult = await _identityService.AddUserToRoleAsync(registerDto.Username!, "Cliente");

                if (roleResult.Succeeded)
                {
                    // Obtener los detalles completos del usuario recién creado
                    var userDetails = await _identityService.FindByNameAsync(registerDto.Username!);

                    // Crear el DTO de Evento para el MR de Reservas
                    var clienteEvent = new ClienteRegistradoEventDto
                    {
                        IdentityId = userDetails.Id,
                        Username = userDetails.UserName,
                        NombreCompleto = registerDto.NombreCompleto!,
                        Email = userDetails.Email,
                        Telefono = registerDto.Telefono!
                    };

                    // Publicar el evento
                    await _messagePublisher.PublishAsync(clienteEvent, "cliente.registrado", "cliente_exchange");

                    return IdentityResult.Success;
                }
            }

            return createdUser;
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
    }
}