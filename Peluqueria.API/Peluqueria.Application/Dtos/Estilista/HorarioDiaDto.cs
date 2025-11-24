using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Estilista
{
    public class HorarioDiaDto // Para HorarioSemanalBase o DescansoFijoDiario
    {
        [Required] public DayOfWeek DiaSemana { get; set; }
        [Required] public TimeSpan HoraInicio { get; set; }
        [Required] public TimeSpan HoraFin { get; set; }
        public bool EsLaborable { get; set; } = true; // Solo para HorarioSemanalBase
    }
}
