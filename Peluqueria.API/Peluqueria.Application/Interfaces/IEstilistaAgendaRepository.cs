using Peluqueria.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IEstilistaAgendaRepository // CAMBIADO a public
    {
        // PEL-HU-12: Horario Base
        Task UpdateHorarioBaseAsync(int estilistaId, List<HorarioSemanalBase> horarios);

        // PEL-HU-13: Bloqueo de Días Libres (Rango)
        Task<BloqueoRangoDiasLibres> CreateBloqueoDiasLibresAsync(BloqueoRangoDiasLibres bloqueo);

        // PEL-HU-13: Descansos Fijos Diarios (Pausas)
        Task UpdateDescansosFijosAsync(int estilistaId, List<BloqueoDescansoFijoDiario> descansos);

        Task<IEnumerable<HorarioSemanalBase>> GetHorarioBaseAsync(int estilistaId); // PEL-HU-12
        Task<IEnumerable<BloqueoRangoDiasLibres>> GetBloqueosDiasLibresAsync(int estilistaId); // PEL-HU-13
        Task<IEnumerable<BloqueoDescansoFijoDiario>> GetDescansosFijosAsync(int estilistaId); // PEL-HU-13
    }
}
