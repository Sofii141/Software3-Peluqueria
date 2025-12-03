namespace Peluqueria.Application.Dtos.Account
{
    /// <summary>
    /// DTO devuelto tras una autenticación o registro exitoso.
    /// Contiene la información básica del usuario y el token de seguridad JWT.
    /// </summary>
    public class NewUserDto
    {
        /// <summary>
        /// Nombre de usuario autenticado.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Token JWT (JSON Web Token) generado para acceder a recursos protegidos.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}