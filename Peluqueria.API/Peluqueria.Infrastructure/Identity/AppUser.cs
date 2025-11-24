using Microsoft.AspNetCore.Identity;

namespace Peluqueria.Infrastructure.Identity
{
    public class AppUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}