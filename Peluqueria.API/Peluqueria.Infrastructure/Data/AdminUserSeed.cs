using Microsoft.AspNetCore.Identity;
using Peluqueria.Infrastructure.Identity;
using System;

namespace Peluqueria.Infrastructure.Data
{
    public static class AdminUserSeed
    {
        private const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
        private const string ADMIN_ROLE_ID = "d17abceb-8c0b-454e-b296-883bc029d82b";

        public static (AppUser, IdentityUserRole<string>) CreateAdminUserWithRole()
        {
            // ... el resto del código es idéntico
            var hasher = new PasswordHasher<AppUser>();

            var adminUser = new AppUser
            {
                Id = ADMIN_ID,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@test.com",
                NormalizedEmail = "ADMIN@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = new Guid().ToString(),
                ConcurrencyStamp = new Guid().ToString()
            };

            adminUser.PasswordHash = hasher.HashPassword(adminUser, "password123");

            var adminUserRole = new IdentityUserRole<string>
            {
                RoleId = ADMIN_ROLE_ID,
                UserId = ADMIN_ID
            };

            return (adminUser, adminUserRole);
        }
    }
}