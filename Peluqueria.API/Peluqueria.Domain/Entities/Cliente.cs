using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Perfil de negocio de un cliente.
    /// </summary>
    /// <remarks>
    /// Al igual que Estilista, esta entidad extiende a <c>AspNetUsers</c> para agregar datos específicos del negocio
    /// que no pertenecen estrictamente a la autenticación (ej. Historial, Preferencias).
    /// </remarks>
    public class Cliente
    {
        public int Id { get; set; }

        /// <summary>
        /// Vinculación con el usuario de Identity (Login).
        /// </summary>
        [Required]
        public string IdentityId { get; set; } = string.Empty;

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;
    }
}