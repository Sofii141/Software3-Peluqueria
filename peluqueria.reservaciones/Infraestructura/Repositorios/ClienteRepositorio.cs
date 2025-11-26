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

        /// <summary>
        /// Busca un cliente por su clave primaria (Identificacion) y devuelve el objeto completo.
        /// </summary>
        public async Task<Cliente?> GetByIdentificationAsync(string identification)
        {
            // Usamos la propiedad de navegación Clientes del DbContext
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Identificacion == identification);
        }

        public async Task SaveOrUpdateAsync(Cliente cliente)
        {
            // 1. Buscamos el cliente por su clave primaria
            var existingCliente = await GetByIdentificationAsync(cliente.Identificacion);

            if (existingCliente == null)
            {
                // Si no existe, es nuevo (CREADO)
                _context.Clientes.Add(cliente);
            }
            else
            {
                // Si existe, actualizamos los campos mapeados (ACTUALIZADO)
                existingCliente.NombreCompleto = cliente.NombreCompleto;
                existingCliente.NombreUsuario = cliente.NombreUsuario;

            }

            //Se persisten los cambios
            await _context.SaveChangesAsync();
        }
    }
}