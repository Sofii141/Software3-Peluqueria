using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Cliente HTTP interno para comunicarse con el Microservicio de Reservas.
    /// </summary>
    /// <remarks>
    /// Se utiliza para consultar validaciones de integridad antes de modificar datos maestros en el Monolito.
    /// </remarks>
    public interface IReservacionClient
    {
        /// <summary>
        /// Verifica si el estilista tiene reservas futuras pendientes.
        /// </summary>
        Task<bool> TieneReservasEstilista(int estilistaId);

        /// <summary>
        /// Verifica si existen reservas futuras para un servicio específico.
        /// </summary>
        Task<bool> TieneReservasServicio(int servicioId);

        /// <summary>
        /// Verifica si existen reservas asociadas a cualquier servicio de una categoría.
        /// </summary>
        Task<bool> TieneReservasCategoria(int categoriaId);

        /// <summary>
        /// Valida si hay reservas en un día específico de la semana (ej. para inhabilitar los Lunes).
        /// </summary>
        Task<bool> TieneReservasEnDia(int estilistaId, System.DayOfWeek dia);

        /// <summary>
        /// Valida si hay reservas en un rango de fechas (ej. para programar vacaciones).
        /// </summary>
        Task<bool> TieneReservasEnRango(int estilistaId, System.DateOnly inicio, System.DateOnly fin);

        /// <summary>
        /// Valida si hay reservas en una franja horaria específica de un día (ej. para poner hora de almuerzo).
        /// </summary>
        Task<bool> TieneReservasEnDescanso(int estilistaId, System.DayOfWeek dia, System.TimeSpan inicio, System.TimeSpan fin);
    }
}