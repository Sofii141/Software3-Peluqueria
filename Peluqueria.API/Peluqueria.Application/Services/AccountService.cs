using Microsoft.AspNetCore.Identity;
using Peluqueria.Application.Dtos.Account;
using Peluqueria.Application.Interfaces;


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