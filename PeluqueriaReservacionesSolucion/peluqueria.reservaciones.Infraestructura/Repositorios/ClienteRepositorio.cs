using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.Persistencia;

namespace peluqueria.reservaciones.Infraestructura.Repositorios
{
    public class ClienteRepositorio : IClienteRepositorio
    {
        private readonly ReservacionesDbContext _context;

        public ClienteRepositorio(ReservacionesDbContext context)
        {
            _context = context;
        }


        public async Task<Cliente?> GetByIdentificationAsync(string identification)
        {
            
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Identificacion == identification);
        }

        public async Task SaveOrUpdateAsync(Cliente cliente)
        {
            var existingCliente = await GetByIdentificationAsync(cliente.Identificacion);

            if (existingCliente == null)
            {
                _context.Clientes.Add(cliente);
            }
            else
            {
                existingCliente.NombreCompleto = cliente.NombreCompleto;
                existingCliente.NombreUsuario = cliente.NombreUsuario;

            }

            await _context.SaveChangesAsync();
        }
    }
}