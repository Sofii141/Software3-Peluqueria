using Microsoft.AspNetCore.Mvc;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Aplicacion.DTO.Comunicacion;

/*
 @autor: Ana sofia Arango
 @descripción: Controlador para validaciones relacionadas con reservaciones.
 */

namespace peluqueria.reservaciones.Api.Controladores
{
    [ApiController]
    [Route("api/validaciones")]
    public class ValidacionesController : ControllerBase
    {
        private readonly IReservacionRepositorio _repo;

        public ValidacionesController(IReservacionRepositorio repo)
        {
            _repo = repo;
        }

        // Validar si un estilista tiene reservas futuras (Para Inactivar Estilista)
        [HttpGet("estilista/{id}")]
        public async Task<IActionResult> CheckEstilista(int id)
            => Ok(await _repo.TieneReservasFuturasEstilistaAsync(id));

        // Validar si un servicio tiene reservas futuras (Para Inactivar/Editar Servicio)
        [HttpGet("servicio/{id}")]
        public async Task<IActionResult> CheckServicio(int id)
            => Ok(await _repo.TieneReservasFuturasServicioAsync(id));

        // Validar si una categoría tiene reservas (Para Inactivar Categoría)
        [HttpGet("categoria/{id}")]
        public async Task<IActionResult> CheckCategoria(int id)
            => Ok(await _repo.TieneReservasFuturasCategoriaAsync(id));

        // Validar si hay reservas en un día completo (Para Marcar día como NO Laborable)
        [HttpGet("estilista/{id}/dia-semana/{dia}")]
        public async Task<IActionResult> CheckDiaSemana(int id, int dia)
            => Ok(await _repo.TieneConflictoHorarioAsync(id, (DayOfWeek)dia));

        // Validar si hay reservas en un rango de fechas (Para Vacaciones/Bloqueos)
        [HttpPost("rango-bloqueo")]
        public async Task<IActionResult> CheckRango([FromBody] PeticionReservasEstilistaDTO dto)
            => Ok(await _repo.TieneConflictoRangoAsync(dto.EstilistaId, dto.FechaInicio, dto.FechaFin));

        [HttpPost("descanso")]
        public async Task<IActionResult> CheckDescanso([FromBody] PeticionDescansoDTO dto)
        {
            var tieneConflicto = await _repo.TieneConflictoDescansoAsync(
                dto.EstilistaId,
                dto.DiaSemana,
                dto.HoraInicio,
                dto.HoraFin);

            return Ok(tieneConflicto);
        }
    }
    public class PeticionDescansoDTO
    {
        public int EstilistaId { get; set; }
        public DayOfWeek DiaSemana { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
    }
}