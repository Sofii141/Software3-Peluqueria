using Peluqueria.Application.Dtos.Estilista;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IEstilistaAgendaService
    {
        // CONSULTAS DE AGENDA (AÑADIDOS)
        Task<IEnumerable<HorarioDiaDto>> GetHorarioBaseAsync(int estilistaId); // PEL-HU-12
        Task<IEnumerable<BloqueoRangoDto>> GetBloqueosDiasLibresAsync(int estilistaId); // PEL-HU-13
        Task<IEnumerable<HorarioDiaDto>> GetDescansosFijosAsync(int estilistaId); // PEL-HU-13

        // OPERACIONES DE AGENDA
        Task<bool> UpdateHorarioBaseAsync(int estilistaId, List<HorarioDiaDto> horarios);
        Task<bool> CreateBloqueoDiasLibresAsync(int estilistaId, BloqueoRangoDto bloqueoDto);
        Task<bool> UpdateDescansoFijoAsync(int estilistaId, List<HorarioDiaDto> descansos);
    }
}
