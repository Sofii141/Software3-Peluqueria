using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Infraestructura.Persistencia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Infraestructura.Repositorios
{
    public class ReservacionRepositorio : IReservacionRepositorio
    {
        private readonly ReservacionesDbContext _context;

        public ReservacionRepositorio(ReservacionesDbContext context)
        {
            _context = context;
        }

        public async Task<Reservacion> GuardarAsync(Reservacion reservacion)
        {
            await _context.Reservaciones.AddAsync(reservacion);
            await _context.SaveChangesAsync();
            return reservacion;
        }

        public async Task<Reservacion?> ObtenerPorIdAsync(int id)
        {
            return await _context.Reservaciones
                .Include(r => r.Cliente)   // Incluimos datos relacionados si es necesario
                .Include(r => r.Servicio)
                .Include(r => r.Estilista)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task ActualizarAsync(Reservacion reservacion)
        {
            // En EF Core, si la entidad ya está rastreada, solo basta con SaveChanges.
            // Si viene desconectada (DTO -> Entidad), usamos Update.
            _context.Reservaciones.Update(reservacion);
            await _context.SaveChangesAsync();
        }

        public async Task CancelarAsync(int id)
        {
            var reservacion = await _context.Reservaciones.FindAsync(id);
            if (reservacion != null)
            {
                reservacion.Estado = "CANCELADA"; // O usa un Enum si lo tienes
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Reservacion>> BuscarReservasPorClienteAsync(string clienteIdentificacion)
        {
            return await _context.Reservaciones
                .Include(r => r.Servicio)
                .Include(r => r.Estilista)
                .Where(r => r.ClienteIdentificacion == clienteIdentificacion)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }

        public async Task<List<Reservacion>> BuscarReservasPorEstilistaAsync(int estilistaId, DateOnly fecha)
        {
            return await _context.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Servicio)
                .Where(r => r.EstilistaId == estilistaId && r.Fecha == fecha && r.Estado != "CANCELADA")
                .OrderBy(r => r.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<Reservacion>> ObtenerReservasConflictivasAsync(int estilistaId, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin)
        {
            // Una reserva existe si:
            // (NuevaInicio < ExistenteFin) Y (NuevaFin > ExistenteInicio)

            return await _context.Reservaciones
                .Where(r =>
                    r.EstilistaId == estilistaId &&
                    r.Fecha == fecha &&
                    r.Estado != "CANCELADA" &&
                    horaInicio < r.HoraFin &&
                    horaFin > r.HoraInicio)
                .ToListAsync();
        }
    }
}