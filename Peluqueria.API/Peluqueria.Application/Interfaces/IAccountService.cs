using Peluqueria.Application.Dtos.Account;
using Microsoft.AspNetCore.Identity;

namespace Peluqueria.Application.Interfaces
{
    public interface IAccountService
    {
        Task<NewUserDto?> LoginAsync(LoginDto loginDto);
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);
        Task<NewUserDto?> GetNewUserDto(string username, string password);
    }
}