using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Events
{
    public class DescansoFijoActualizadoEventDto
    {
        public int EstilistaId { get; set; }
        public List<DiaHorarioEventDto> DescansosFijos { get; set; } = new List<DiaHorarioEventDto>();
    }

}
