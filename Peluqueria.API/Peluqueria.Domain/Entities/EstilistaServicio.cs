namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Entidad de unión para la relación Muchos a Muchos entre <see cref="Estilista"/> y <see cref="Servicio"/>.
    /// </summary>
    /// <remarks>
    /// Representa la "Habilidad": Un estilista sabe hacer un servicio específico.
    /// En Base de Datos, esto se convierte en una tabla con clave compuesta (EstilistaId, ServicioId).
    /// </remarks>
    public class EstilistaServicio
    {
        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;

        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;
    }
}