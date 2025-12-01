using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")] // ¡Importante! Solo admins
    public class AdminController : ControllerBase
    {
        private readonly IDataSyncService _syncService;

        public AdminController(IDataSyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost("sincronizar-datos")]
        public async Task<IActionResult> SincronizarDatos()
        {
            await _syncService.SincronizarTodoAsync();
            return Ok(new { mensaje = "Inicio de sincronización masiva de datos hacia el microservicio." });
        }
    }
}