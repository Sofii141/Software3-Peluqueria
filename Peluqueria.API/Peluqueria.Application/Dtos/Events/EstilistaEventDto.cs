namespace Peluqueria.Application.Dtos.Events
{
    /// <summary>
    /// Evento masivo que contiene toda la información perfil y habilidades de un Estilista.
    /// </summary>
    /// <remarks>
    /// Se envía cuando se crea o modifica un estilista. Incluye la lista de servicios que puede realizar.
    /// Exchange: `estilista_exchange`.
    /// </remarks>
    public class EstilistaEventDto
    {
        /// <summary>
        /// ID numérico (PK) del estilista.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// GUID del sistema de identidad (AspNetUsers) para enlazar con la autenticación.
        /// </summary>
        public string IdentityId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del profesional.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico de contacto.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono de contacto.
        /// </summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Estado operativo del estilista. Si es false, no debe recibir citas.
        /// </summary>
        public bool EstaActivo { get; set; }

        /// <summary>
        /// Nombre del archivo de imagen (o URL) para mostrar en el frontend de reservas.
        /// </summary>
        public string ImagenUrl { get; set; } = string.Empty;

        /// <summary>
        /// Lista de servicios que este estilista está capacitado para realizar.
        /// </summary>
        public List<EstilistaServicioMinimalEventDto> ServiciosAsociados { get; set; } = new List<EstilistaServicioMinimalEventDto>();

        /// <summary>
        /// Acción realizada: "CREADO", "ACTUALIZADO", "INACTIVADO".
        /// </summary>
        public string Accion { get; set; } = string.Empty;
    }
}