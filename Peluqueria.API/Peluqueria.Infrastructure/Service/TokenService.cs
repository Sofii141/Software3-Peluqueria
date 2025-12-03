using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Account;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Peluqueria.Infrastructure.Service
{
    /// <summary>
    /// Servicio de seguridad encargado de generar Tokens JWT.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            var signingKey = _config["JWT:SigningKey"];
            if (string.IsNullOrEmpty(signingKey))
            {
                throw new ArgumentNullException(nameof(signingKey), "JWT:SigningKey not configured in appsettings.json");
            }
            // La clave debe ser de al menos 64 bytes para HMAC-SHA512
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        }

        /// <summary>
        /// Crea un token JWT firmado digitalmente.
        /// </summary>
        /// <param name="user">Datos básicos del usuario autenticado.</param>
        /// <param name="roles">Lista de roles asignados al usuario.</param>
        /// <returns>El string del token JWT listo para devolver al cliente.</returns>
        public string CreateToken(AppUserMinimalDto user, IList<string> roles)
        {
            // 1. Definir los Claims (Datos incrustados en el token)
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty), // Usado por Identity para User.Identity.Name
                new(JwtRegisteredClaimNames.NameId, user.Id ?? string.Empty),     // El ID del usuario
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };

            // Agregamos los roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 2. Definir credenciales de firma (Algoritmo HmacSha512)
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 3. Configurar el descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7), // Validez de 1 semana
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            // 4. Generar el token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}