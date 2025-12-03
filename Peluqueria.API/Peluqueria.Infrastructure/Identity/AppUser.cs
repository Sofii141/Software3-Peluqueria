using Microsoft.AspNetCore.Identity;

namespace Peluqueria.Infrastructure.Identity
{
    /// <summary>
    /// Clase de identidad personalizada que extiende <see cref="IdentityUser"/>.
    /// </summary>
    /// <remarks>
    /// Mapea a la tabla SQL <c>AspNetUsers</c>. 
    /// Se agregan propiedades personalizadas para evitar crear una tabla de "Perfil" separada,
    /// centralizando los datos de autenticación y datos básicos del usuario en un solo lugar.
    /// </remarks>
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// Nombre y apellido real del usuario (diferente al Username de login).
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Número de contacto principal.
        /// </summary>
        public string Telefono { get; set; } = string.Empty;
    }
}