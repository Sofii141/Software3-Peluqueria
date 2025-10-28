using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Peluqueria.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        // Cuando crees la entidad "Reserva", añadiremos algo como:
        // public List<Reserva> ReservasComoCliente { get; set; } = new List<Reserva>();
        // public List<Reserva> ReservasComoEstilista { get; set; } = new List<Reserva>();
    }
}