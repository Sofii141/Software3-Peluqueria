using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Domain.Entities
{
    public class BloqueoDescansoFijoDiario
    {
        public int Id { get; set; }

        // Clave foránea al estilista
        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;

        // Lo modelamos por día de la semana, asumiendo que el descanso es recurrente (ej. Almuerzo).
        public DayOfWeek DiaSemana { get; set; }

        // El rango de tiempo que se bloquea
        public TimeSpan HoraInicioDescanso { get; set; }
        public TimeSpan HoraFinDescanso { get; set; }

        public string Razon { get; set; } = "Pausa/Almuerzo";
    }
}
