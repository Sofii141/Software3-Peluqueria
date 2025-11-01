// Peluqueria.Application/Services/AccountService.cs (Corregido)
using Microsoft.AspNetCore.Identity;
using Peluqueria.Application.Dtos.Account;
using Peluqueria.Application.Interfaces;
// using Peluqueria.Domain.Entities; <-- Ya no es necesario

namespace Peluqueria.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;

        public AccountService(IIdentityService identityService, ITokenService tokenService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
        }

        public async Task<NewUserDto?> LoginAsync(LoginDto loginDto)
        {
            var userMinimal = await _identityService.GetUserByUsernameAsync(loginDto.Username.ToLower());

            if (userMinimal == null)
            {
                return null;
            }

            // checkPasswordSignInAsync ahora recibe el username
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
                Token = _tokenService.CreateToken(userMinimal, roles) // Usa el DTO minimal
            };
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
        {
            // IdentityService ahora recibe solo las primitivas
            var createdUser = await _identityService.CreateUserAsync(registerDto.Username!, registerDto.Email!, registerDto.Password!);

            if (createdUser.Succeeded)
            {
                // IdentityService ahora recibe solo el username
                var roleResult = await _identityService.AddUserToRoleAsync(registerDto.Username!, "Cliente");

                if (roleResult.Succeeded)
                {
                    return IdentityResult.Success;
                }
                else
                {
                    return roleResult;
                }
            }

            return createdUser;
        }

        public async Task<NewUserDto?> GetNewUserDto(string username, string password)
        {
            // IdentityService ahora devuelve el DTO minimal
            var userMinimal = await _identityService.FindByNameAsync(username);
            if (userMinimal == null) return null;

            // IdentityService ahora recibe el username
            var roles = await _identityService.GetRolesAsync(userMinimal.UserName);

            return new NewUserDto
            {
                UserName = userMinimal.UserName!,
                Email = userMinimal.Email!,
                Token = _tokenService.CreateToken(userMinimal, roles) // Usa el DTO minimal
            };
        }
    }
}