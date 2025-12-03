using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Peluqueria.Application.Extensions
{
    /// <summary>
    /// Clases de extensión para facilitar la extracción de datos del usuario autenticado (`ClaimsPrincipal`).
    /// </summary>
    /// <remarks>
    /// Evita repetir lógica LINQ en los controladores para leer los claims del Token JWT.
    /// </remarks>
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Obtiene el nombre de usuario (GivenName) desde los claims del usuario actual.
        /// </summary>
        /// <remarks>
        /// Este método busca específicamente el claim con la URI estándar: 
        /// <c>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname</c>.
        /// <br/>
        /// Útil para recuperar el <c>username</c> dentro de un Controller sin acceder a la base de datos.
        /// </remarks>
        /// <param name="user">El objeto <see cref="ClaimsPrincipal"/> (User) del contexto HTTP actual.</param>
        /// <returns>El valor del nombre de usuario (string) contenido en el token.</returns>
        /// <exception cref="NullReferenceException">Si el usuario no tiene el claim esperado (no está autenticado correctamente).</exception>
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // Nota: Se asume que el usuario está autenticado y tiene el claim. 
            // Si el claim es nulo, .Value lanzará excepción.
            return user.Claims.SingleOrDefault(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")).Value;
        }
    }
}