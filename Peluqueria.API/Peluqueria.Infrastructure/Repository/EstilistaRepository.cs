using Microsoft.EntityFrameworkCore;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Infrastructure.Repository
{
    /// <summary>
    /// Gestión de datos de Estilistas y sus relaciones (Servicios).
    /// </summary>
    public class EstilistaRepository : IEstilistaRepository
    {
        private readonly ApplicationDBContext _context;

        public EstilistaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los estilistas (excepto el Administrador principal).
        /// </summary>
        /// <remarks>
        /// Incluye la relación <c>ServiciosAsociados</c> para mostrar las especialidades en la lista.
        /// </remarks>
        public async Task<IEnumerable<Estilista>> GetAllAsync()
        {
            return await _context.Estilistas
               .Where(e => e.IdentityId != AdminUserSeed.ADMIN_ID) // Filtro de seguridad
               .Include(e => e.ServiciosAsociados)
               .ThenInclude(es => es.Servicio)
               .ToListAsync();
        }

        public async Task<Estilista?> GetByIdentityIdAsync(string identityId)
        {
            return await _context.Estilistas
                .Include(e => e.ServiciosAsociados)
                .ThenInclude(es => es.Servicio)
                .FirstOrDefaultAsync(e => e.IdentityId == identityId);
        }

        /// <summary>
        /// Carga "pesada" de un estilista con TODAS sus relaciones de agenda.
        /// </summary>
        /// <remarks>
        /// Utiliza <c>Include</c> para traer HorariosBase, Bloqueos y Descansos en una sola consulta.
        /// Ideal para la edición completa del perfil.
        /// </remarks>
        public async Task<Estilista?> GetFullEstilistaByIdAsync(int id)
        {
            return await _context.Estilistas
                .Include(e => e.ServiciosAsociados)
                    .ThenInclude(es => es.Servicio)
                .Include(e => e.HorariosBase)
                .Include(e => e.BloqueosRangoDiasLibres)
                .Include(e => e.BloqueosDescansoFijoDiario)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Crea un estilista y mapea manualmente la tabla intermedia de Servicios.
        /// </summary>
        public async Task<Estilista> CreateAsync(Estilista estilista, List<int> serviciosIds)
        {
            // Mapeo manual de la relación Muchos a Muchos
            estilista.ServiciosAsociados = serviciosIds.Select(id => new EstilistaServicio
            {
                ServicioId = id,
                Estilista = estilista
            }).ToList();

            await _context.Estilistas.AddAsync(estilista);
            await _context.SaveChangesAsync();

            return estilista;
        }

        /// <summary>
        /// Actualiza el estilista y reconstruye sus asociaciones de servicios.
        /// </summary>
        public async Task<Estilista?> UpdateAsync(Estilista estilista, List<int> serviciosIds)
        {
            var existingEstilista = await _context.Estilistas
                .Include(e => e.ServiciosAsociados)
                .FirstOrDefaultAsync(e => e.Id == estilista.Id);

            if (existingEstilista == null) return null;

            existingEstilista.NombreCompleto = estilista.NombreCompleto;
            existingEstilista.Telefono = estilista.Telefono;
            existingEstilista.EstaActivo = estilista.EstaActivo; // Permite la baja lógica

            // Estrategia: Borrar relaciones anteriores y crear nuevas
            _context.EstilistaServicios.RemoveRange(existingEstilista.ServiciosAsociados);

            var newAssociations = serviciosIds.Select(id => new EstilistaServicio
            {
                EstilistaId = estilista.Id,
                ServicioId = id
            }).ToList();

            await _context.EstilistaServicios.AddRangeAsync(newAssociations);

            await _context.SaveChangesAsync();

            // Refrescamos el objeto en memoria para el retorno
            existingEstilista.ServiciosAsociados = newAssociations;

            return existingEstilista;
        }
    }
}