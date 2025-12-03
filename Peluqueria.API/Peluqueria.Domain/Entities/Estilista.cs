using System.Collections.Generic;

namespace Peluqueria.Domain.Entities
{
    /// <summary>
    /// Representa a un empleado/profesional que realiza servicios.
    /// </summary>
    /// <remarks>
    /// Esta entidad extiende la información del usuario de login. 
    /// Mientras que <c>AppUser</c> (Identity) maneja el login, esta entidad maneja la agenda y comisiones.
    /// </remarks>
    public class Estilista
    {
        public int Id { get; set; }

        /// <summary>
        /// Clave foránea lógica hacia la tabla <c>AspNetUsers</c>. 
        /// Vincula este perfil con la cuenta de autenticación.
        /// </summary>
        public string IdentityId { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Estado operativo. Si es false, el estilista no aparece disponible para agendar citas.
        /// </summary>
        public bool EstaActivo { get; set; } = true;

        public string Imagen { get; set; } = string.Empty;

        // --- RELACIONES ---

        /// <summary>
        /// Habilidades: Lista de servicios que este estilista puede realizar (Tabla intermedia).
        /// </summary>
        public ICollection<EstilistaServicio> ServiciosAsociados { get; set; } = new List<EstilistaServicio>();

        /// <summary>
        /// Configuración de su jornada laboral semanal estándar (Lunes a Domingo).
        /// </summary>
        public ICollection<HorarioSemanalBase> HorariosBase { get; set; } = new List<HorarioSemanalBase>();

        /// <summary>
        /// Excepciones de calendario: Vacaciones, incapacidades o días libres puntuales.
        /// </summary>
        public ICollection<BloqueoRangoDiasLibres> BloqueosRangoDiasLibres { get; set; } = new List<BloqueoRangoDiasLibres>();

        /// <summary>
        /// Descansos recurrentes intra-jornada (ej. Hora de almuerzo diaria).
        /// </summary>
        public ICollection<BloqueoDescansoFijoDiario> BloqueosDescansoFijoDiario { get; set; } = new List<BloqueoDescansoFijoDiario>();
    }
}