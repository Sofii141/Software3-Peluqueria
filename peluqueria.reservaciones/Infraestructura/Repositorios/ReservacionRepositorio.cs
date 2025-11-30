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

            var existeCliente = await _context.Clientes
                    .AsNoTracking()
                    .AnyAsync(c => c.Identificacion == reservacion.ClienteIdentificacion);

            if (existeCliente)
            {
                reservacion.Cliente = null;
            }
            else
            {

                if (reservacion.Cliente != null)
                {

                    reservacion.Cliente.Identificacion = reservacion.ClienteIdentificacion;

                    _context.Clientes.Add(reservacion.Cliente);
                }
            }

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
                .Where(r => r.EstilistaId == estilistaId && r.Fecha == fecha && r.Estado == "PENDIENTE")
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

        public async Task CambiarEstadoAsync(int reservacionId, string nuevoEstado)
        {
            var reservacion = await _context.Reservaciones.FindAsync(reservacionId);
            if (reservacion != null)
            {
                reservacion.Estado = nuevoEstado;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Reservacion>> BuscarReservasEstilistaRangoAsync(int estilistaId, DateOnly fechaInicio, DateOnly fechaFin)
        {
            return await _context.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Servicio)
                .Where(r =>
                    r.EstilistaId == estilistaId &&
                    r.Fecha >= fechaInicio &&
                    r.Fecha <= fechaFin &&
                    r.Estado != "PENDIENTE")
                .OrderBy(r => r.Fecha)
                .ThenBy(r => r.HoraInicio)
                .ToListAsync();
        }

        public async Task<bool> TieneReservasFuturasEstilistaAsync(int estilistaId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now); 
            return await _context.Reservaciones
                .AnyAsync(r => r.EstilistaId == estilistaId && r.Fecha >= hoy && r.Estado != "CANCELADA");
        }

        public async Task<bool> TieneReservasFuturasServicioAsync(int servicioId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            return await _context.Reservaciones
                .AnyAsync(r => r.ServicioId == servicioId && r.Fecha >= hoy && r.Estado != "CANCELADA");
        }

        public async Task<bool> TieneReservasFuturasCategoriaAsync(int categoriaId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            return await _context.Reservaciones
                .Include(r => r.Servicio)
                .AnyAsync(r => r.Servicio.CategoriaId == categoriaId && r.Fecha >= hoy && r.Estado != "CANCELADA");
        }

        public async Task<bool> TieneConflictoHorarioAsync(int estilistaId, DayOfWeek diaSemana)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var fechasReservadas = await _context.Reservaciones
                .Where(r => r.EstilistaId == estilistaId && r.Fecha >= hoy && r.Estado != "CANCELADA")
                .Select(r => r.Fecha)
                .ToListAsync();

            return fechasReservadas.Any(f => f.ToDateTime(TimeOnly.MinValue).DayOfWeek == diaSemana);
        }

        public async Task<bool> TieneConflictoRangoAsync(int estilistaId, DateOnly inicio, DateOnly fin)
        {
            return await _context.Reservaciones
                .AnyAsync(r => r.EstilistaId == estilistaId
                               && r.Fecha >= inicio
                               && r.Fecha <= fin
                               && r.Estado != "CANCELADA");
        }

        public async Task<bool> TieneConflictoDescansoAsync(int estilistaId, DayOfWeek dia, TimeOnly inicio, TimeOnly fin)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            // 1. Traemos las reservas futuras de ese estilista (Traemos fecha y hora para filtrar en memoria)
            // Nota: EF Core no traduce bien DayOfWeek en todas las versiones de SQL, por eso filtramos en cliente.
            var reservasFuturas = await _context.Reservaciones
                .Where(r => r.EstilistaId == estilistaId && r.Fecha >= hoy && r.Estado != "CANCELADA")
                .Select(r => new { r.Fecha, r.HoraInicio, r.HoraFin })
                .ToListAsync();

            // 2. Verificamos en memoria:
            // - Que la fecha caiga en ese día de la semana (ej: Lunes)
            // - Que las horas se solapen
            return reservasFuturas.Any(r =>
                r.Fecha.ToDateTime(TimeOnly.MinValue).DayOfWeek == dia && // Coincide el día
                r.HoraInicio < fin && r.HoraFin > inicio // Hay solapamiento de horas
            );
        }

    }
}