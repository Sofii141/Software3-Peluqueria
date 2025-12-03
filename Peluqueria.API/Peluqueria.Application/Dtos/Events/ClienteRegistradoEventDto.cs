namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Evento disparado cuando un nuevo cliente se registra en el sistema.
    /// </summary>
    /// <remarks>
    /// Permite al microservicio crear una réplica local del cliente para asociarle reservas.
    /// Exchange: `cliente_exchange`.
    /// </remarks>
    public class ClienteRegistradoEventDto
    {
        /// <summary>
        /// ID (GUID) generado por Identity. Clave principal global del usuario.
        /// </summary>
        public string IdentityId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario (login).
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Nombre real del cliente.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono celular.
        /// </summary>
        public string Telefono { get; set; } = string.Empty;
    }
}