using peluqueria.reservaciones.Core.Dominio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
	public interface IHorarioRepositorio
	{
		Task SetBaseScheduleAsync(int stylistId, List<DiaHorario> schedules);
		Task SetFixedBreaksAsync(int stylistId, List<DiaHorario> fixedBreaks);
		Task AddBlockoutRangeAsync(BloqueoRangoDiasLibres blockout);
		Task<HorarioBase?> GetStylistScheduleAsync(int stylistId);
	}
}