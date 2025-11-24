using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Domain.Entities
{
    public class BloqueoRangoDiasLibres
    {
        public int Id { get; set; }

        // Clave foránea al estilista bloqueado
        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;

        // Inicio del rango (vacaciones, etc.)
        public DateTime FechaInicioBloqueo { get; set; }

        // Fin del rango (vacaciones, etc.)
        public DateTime FechaFinBloqueo { get; set; }

        // Razón del bloqueo (opcional, para trazabilidad)
        public string Razon { get; set; } = "Día Libre/Vacaciones";
    }
}
