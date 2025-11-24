using Peluqueria.Application.Interfaces;
using Peluqueria.Infrastructure.Identity;
using Peluqueria.Application.Dtos.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Peluqueria.Infrastructure.Service
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public IdentityService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AppUserMinimalDto?> FindByIdentityIdAsync(string identityId)
        {
            var user = await _userManager.FindByIdAsync(identityId);
            return user == null ? null : MapToMinimalDto(user);
        }

        private static AppUserMinimalDto MapToMinimalDto(AppUser user)
        {
            return new AppUserMinimalDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!
            };
        }

        public async Task<AppUserMinimalDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName!.ToLower() == username);
            return user == null ? null : MapToMinimalDto(user);
        }

        public async Task<IList<string>> GetRolesAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return new List<string>();
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<SignInResult> CheckPasswordSignInAsync(string username, string password, bool lockoutOnFailure)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return SignInResult.Failed;
            return await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
        }

        public async Task<IdentityResult> CreateUserAsync(string username, string email, string password, string nombreCompleto, string telefono)
        {
            var appUser = new AppUser
            {
                UserName = username.ToLower(),
                Email = email,
                NombreCompleto = nombreCompleto,
                Telefono = telefono
            };
            return await _userManager.CreateAsync(appUser, password);
        }

        public async Task<IdentityResult> AddUserToRoleAsync(string username, string roleName)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = $"User {username} not found." });
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<AppUserMinimalDto?> FindByNameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user == null ? null : MapToMinimalDto(user);
        }


    }
}