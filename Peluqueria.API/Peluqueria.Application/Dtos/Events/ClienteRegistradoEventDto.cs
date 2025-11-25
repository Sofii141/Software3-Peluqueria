namespace Peluqueria.Application.Dtos.Events
{
    public class ClienteRegistradoEventDto
    {
        public string IdentityId { get; set; } = string.Empty; // <<-- AÑADIDO
        public string Username { get; set; } = string.Empty; // <<-- AÑADIDO
        public string NombreCompleto { get; set; } = string.Empty; // <<-- AÑADIDO
        public string Email { get; set; } = string.Empty; // <<-- AÑADIDO
        public string Telefono { get; set; } = string.Empty; // <<-- AÑADIDO
    }
}
