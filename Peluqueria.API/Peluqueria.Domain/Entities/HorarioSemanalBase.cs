using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Domain.Entities
{
    public class HorarioSemanalBase
    {
        public int Id { get; set; }

        // Clave foránea al estilista que tiene esta configuración
        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;

        [Required]
        public DayOfWeek DiaSemana { get; set; } // Lunes, Martes, Sábado, Domingo, etc.

        // La hora de fin de la jornada debe ser posterior a la de inicio.
        public TimeSpan HoraInicioJornada { get; set; } // Hora de inicio del día de trabajo
        public TimeSpan HoraFinJornada { get; set; } // Hora de fin del día de trabajo

        // Campo para indicar si el día está marcado como no laborable en la configuración base.
        public bool EsLaborable { get; set; } = true;
    }
}
