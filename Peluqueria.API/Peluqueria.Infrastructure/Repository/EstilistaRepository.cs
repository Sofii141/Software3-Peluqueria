using Microsoft.EntityFrameworkCore;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Data;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Infrastructure.Repository
{
    public class EstilistaRepository : IEstilistaRepository
    {
        private readonly ApplicationDBContext _context;

        public EstilistaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Estilista>> GetAllAsync()
        {
             return await _context.Estilistas
                .Where(e => e.IdentityId != AdminUserSeed.ADMIN_ID) // <--- FILTRO POR ID DE IDENTITY
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


        public async Task<Estilista?> GetFullEstilistaByIdAsync(int id)
        {
            return await _context.Estilistas
                .Include(e => e.ServiciosAsociados)
                    .ThenInclude(es => es.Servicio)
                .Include(e => e.HorariosBase)
                .Include(e => e.BloqueosRangoDiasLibres)
                .Include(e => e.BloqueosDescansoFijoDiario) // Debe coincidir con el DbSet
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<Estilista> CreateAsync(Estilista estilista, List<int> serviciosIds)
        {
          
            estilista.ServiciosAsociados = serviciosIds.Select(id => new EstilistaServicio
            {
                ServicioId = id,
                Estilista = estilista 
            }).ToList();

            await _context.Estilistas.AddAsync(estilista);
            await _context.SaveChangesAsync();

            return estilista;
        }

        public async Task<Estilista?> UpdateAsync(Estilista estilista, List<int> serviciosIds)
        {
            var existingEstilista = await _context.Estilistas
                .Include(e => e.ServiciosAsociados)
                .FirstOrDefaultAsync(e => e.Id == estilista.Id);

            if (existingEstilista == null) return null;

            existingEstilista.NombreCompleto = estilista.NombreCompleto;
            existingEstilista.Telefono = estilista.Telefono;
            existingEstilista.EstaActivo = estilista.EstaActivo; // Importante para la Baja Lógica

            _context.EstilistaServicios.RemoveRange(existingEstilista.ServiciosAsociados);

            var newAssociations = serviciosIds.Select(id => new EstilistaServicio
            {
                EstilistaId = estilista.Id,
                ServicioId = id
            }).ToList();

            await _context.EstilistaServicios.AddRangeAsync(newAssociations);

            await _context.SaveChangesAsync();


            existingEstilista.ServiciosAsociados = newAssociations;

            return existingEstilista;
        }


    }
}
