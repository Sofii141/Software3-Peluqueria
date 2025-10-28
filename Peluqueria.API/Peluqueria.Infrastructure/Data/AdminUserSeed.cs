using Microsoft.AspNetCore.Identity;
using Peluqueria.Domain.Entities;
using System; 

namespace Peluqueria.Infrastructure.Data
{
    public static class AdminUserSeed
    {
        private const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
        private const string ADMIN_ROLE_ID = "d17abceb-8c0b-454e-b296-883bc029d82b";

        public static (AppUser, IdentityUserRole<string>) CreateAdminUserWithRole()
        {
            // 1. Se crea la herramienta para encriptar
            var hasher = new PasswordHasher<AppUser>();

            // 2. Se crea el objeto de usuario
            var adminUser = new AppUser
            {
                Id = ADMIN_ID,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@test.com",
                NormalizedEmail = "ADMIN@TEST.COM",
                EmailConfirmed = true,
                // Estos sellos son necesarios para que Identity funcione correctamente
                SecurityStamp = new Guid().ToString(),
                ConcurrencyStamp = new Guid().ToString()
            };

            // 3. Se encripta la contraseña y se asigna al campo PasswordHash
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "password123");

            // 4. Se crea la relación con el rol
            var adminUserRole = new IdentityUserRole<string>
            {
                RoleId = ADMIN_ROLE_ID,
                UserId = ADMIN_ID
            };

            return (adminUser, adminUserRole);
        }
    }
}