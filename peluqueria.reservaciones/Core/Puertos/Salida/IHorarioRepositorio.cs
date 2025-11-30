using peluqueria.reservaciones.Core.Dominio;
using System.Collections.Generic;
using System.Threading.Tasks;

/*
 @author: Juan David Moran
	@description: Interfaz para el repositorio referente a los horarios de los estilista, inclute horario base, dia horario,
	descanso fijo y bloque de un rango de dias.
 */

namespace peluqueria.reservaciones.Core.Puertos.Salida
{
	public interface IHorarioRepositorio
	{
		Task SetBaseScheduleAsync(int stylistId, List<DiaHorario> schedules);
		Task SetFixedBreaksAsync(int stylistId, List<DiaHorario> fixedBreaks);
		Task AddBlockoutRangeAsync(BloqueoRangoDiasLibres blockout);
		Task<HorarioBase?> GetStylistScheduleAsync(int stylistId);
		Task<BloqueoRangoDiasLibres?> GetRangoDiasLibres(int stylistId);
		Task<DescansoFijo?> GetDescanso(int stylistId);
	}
}