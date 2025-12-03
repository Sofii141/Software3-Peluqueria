using Peluqueria.Application.Dtos.Account;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Servicio encargado de la generación y firma de tokens de seguridad (JWT).
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Crea un JWT firmado para un usuario con sus roles y claims.
        /// </summary>
        string CreateToken(AppUserMinimalDto user, IList<string> roles);
    }
}