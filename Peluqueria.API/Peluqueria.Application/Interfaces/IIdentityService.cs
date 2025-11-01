using Microsoft.AspNetCore.Identity;
using Peluqueria.Application.Dtos.Account; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<AppUserMinimalDto?> GetUserByUsernameAsync(string username);
        Task<IList<string>> GetRolesAsync(string username);
        Task<SignInResult> CheckPasswordSignInAsync(string username, string password, bool lockoutOnFailure);
        Task<IdentityResult> CreateUserAsync(string username, string email, string password);
        Task<IdentityResult> AddUserToRoleAsync(string username, string roleName);
        Task<AppUserMinimalDto?> FindByNameAsync(string username);
    }
}