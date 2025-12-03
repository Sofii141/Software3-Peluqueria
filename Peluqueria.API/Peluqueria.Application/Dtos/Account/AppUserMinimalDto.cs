namespace Peluqueria.Application.Dtos.Account
{
    /// <summary>
    /// Representación reducida de un usuario del sistema, utilizada para referencias ligeras o internas.
    /// </summary>
    public class AppUserMinimalDto
    {
        /// <summary>
        /// Identificador único del usuario (GUID).
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario único en el sistema.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico asociado a la cuenta.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}