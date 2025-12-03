namespace Peluqueria.Application.Dtos.Account
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para devolver la información de perfil de los clientes.
    /// </summary>
    public class ClienteResponseDto
    {
        /// <summary>
        /// Identificador único del cliente.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario del cliente.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Nombre y apellido completos del cliente.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico de contacto.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Número telefónico de contacto.
        /// </summary>
        public string Telefono { get; set; } = string.Empty;
    }
}