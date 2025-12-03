namespace Peluqueria.Application.Dtos.Estilista
{
    /// <summary>
    /// Representa la información pública de un estilista en el sistema.
    /// </summary>
    public class EstilistaDto
    {
        /// <summary>
        /// Identificador único del estilista en la base de datos.
        /// </summary>
        /// <example>15</example>
        public int Id { get; set; }

        /// <summary>
        /// Nombre completo del profesional.
        /// </summary>
        /// <example>Laura Valencia</example>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico de contacto.
        /// </summary>
        /// <example>laura@peluqueria.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono celular de contacto.
        /// </summary>
        /// <example>3001234567</example>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el estilista está activo en el sistema (false = eliminado lógicamente).
        /// </summary>
        /// <example>true</example>
        public bool EstaActivo { get; set; }

        /// <summary>
        /// URL completa para acceder a la imagen de perfil del estilista.
        /// </summary>
        /// <example>https://api.mipeluqueria.com/images/estilistas/foto1.jpg</example>
        public string Imagen { get; set; } = string.Empty;

        /// <summary>
        /// Lista de IDs de los servicios que este estilista está capacitado para realizar.
        /// </summary>
        /// <example>[1, 2, 5]</example>
        public List<int> ServiciosIds { get; set; } = new List<int>();
    }
}