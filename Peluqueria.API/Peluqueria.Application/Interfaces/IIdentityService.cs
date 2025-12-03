using Microsoft.AspNetCore.Identity;
using Peluqueria.Application.Dtos.Account;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Capa de abstracción sobre ASP.NET Core Identity.
    /// </summary>
    /// <remarks>
    /// Encapsula la complejidad de UserManager y SignInManager para desacoplar la lógica de negocio.
    /// </remarks>
    public interface IIdentityService
    {
        Task<AppUserMinimalDto?> GetUserByUsernameAsync(string username);
        Task<IList<string>> GetRolesAsync(string username);
        Task<SignInResult> CheckPasswordSignInAsync(string username, string password, bool lockoutOnFailure);
        Task<IdentityResult> AddUserToRoleAsync(string username, string roleName);
        Task<AppUserMinimalDto?> FindByNameAsync(string username);
        Task<IdentityResult> CreateUserAsync(string username, string email, string password, string nombreCompleto, string telefono);
        Task<AppUserMinimalDto?> FindByIdentityIdAsync(string identityId);

        /// <summary>
        /// Permite a un administrador resetear la contraseña de un usuario sin conocer la anterior.
        /// </summary>
        Task<IdentityResult> AdminResetPasswordAsync(string identityId, string newPassword);

        /// <summary>
        /// Actualiza username o email asegurando unicidad.
        /// </summary>
        Task<IdentityResult> UpdateUserCredentialsAsync(string identityId, string newUsername, string newEmail);

        Task<IEnumerable<ClienteResponseDto>> GetUsersByRoleAsync(string roleName);
    }
}