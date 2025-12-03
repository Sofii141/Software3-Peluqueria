using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Infrastructure.HttpClients
{
    /// <summary>
    /// Cliente HTTP encargado de la comunicación sincrónica con el Microservicio de Reservas.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa una estrategia de **"Fail-Safe" (Fallo Seguro)**:
    /// Si la comunicación con el microservicio falla (TimeOut, 500, Red caída), los métodos retornan <c>true</c>.
    /// <br/>
    /// <b>Justificación de Diseño:</b> 
    /// Es más seguro bloquear una operación administrativa (como eliminar un estilista) asumiendo erróneamente 
    /// que tiene reservas, a permitir la eliminación y dejar la base de datos de reservas corrupta o inconsistente.
    /// </remarks>
    public class ReservacionClient : IReservacionClient
    {
        private readonly HttpClient _httpClient;

        public ReservacionClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Método auxiliar para ejecutar peticiones GET y procesar la respuesta booleana de forma segura.
        /// </summary>
        /// <param name="url">Endpoint relativo a consultar.</param>
        /// <returns>
        /// <c>false</c> solo si el microservicio confirma explícitamente que NO hay reservas.
        /// <c>true</c> si hay reservas O si ocurre cualquier error de conexión.
        /// </returns>
        private async Task<bool> GetBoolAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                // Si el microservicio devuelve 404 o 500, asumimos true por seguridad.
                if (!response.IsSuccessStatusCode) return true;

                var content = await response.Content.ReadAsStringAsync();
                return bool.Parse(content);
            }
            catch
            {
                // En caso de excepción (Microservicio caído), activamos el Fail-Safe.
                return true;
            }
        }

        /// <summary>
        /// Consulta si existen reservas futuras asociadas a un estilista.
        /// </summary>
        public async Task<bool> TieneReservasEstilista(int id)
            => await GetBoolAsync($"api/validaciones/estilista/{id}");

        /// <summary>
        /// Consulta si existen reservas futuras asociadas a un servicio específico.
        /// </summary>
        public async Task<bool> TieneReservasServicio(int id)
            => await GetBoolAsync($"api/validaciones/servicio/{id}");

        /// <summary>
        /// Consulta si existen reservas futuras que involucren cualquier servicio de una categoría.
        /// </summary>
        public async Task<bool> TieneReservasCategoria(int id)
            => await GetBoolAsync($"api/validaciones/categoria/{id}");

        /// <summary>
        /// Valida si hay reservas en un día específico de la semana (ej. Lunes).
        /// Útil antes de marcar un día como "No Laborable".
        /// </summary>
        public async Task<bool> TieneReservasEnDia(int id, DayOfWeek dia)
            => await GetBoolAsync($"api/validaciones/estilista/{id}/dia-semana/{(int)dia}");

        /// <summary>
        /// Valida si hay reservas en un rango de fechas calendario.
        /// Útil antes de aprobar unas vacaciones (Bloqueo).
        /// </summary>
        /// <remarks>
        /// Serializa las fechas usando <see cref="DateOnly"/> para compatibilidad con el endpoint del microservicio.
        /// </remarks>
        public async Task<bool> TieneReservasEnRango(int id, DateOnly inicio, DateOnly fin)
        {
            try
            {
                var body = new { EstilistaId = id, FechaInicio = inicio, FechaFin = fin };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Asegúrate que esta ruta coincide con el controlador del microservicio
                var response = await _httpClient.PostAsync("api/validaciones/rango-bloqueo", content);

                if (!response.IsSuccessStatusCode) return true;

                var result = await response.Content.ReadAsStringAsync();
                return bool.Parse(result);
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Valida si hay reservas en una franja horaria específica de un día (ej. Hora de almuerzo).
        /// </summary>
        /// <remarks>
        /// Realiza una conversión de datos crítica: 
        /// Transforma el <see cref="TimeSpan"/> (usado en Monolito/SQLServer) a <see cref="TimeOnly"/> (usado en Microservicio/.NET 8)
        /// para asegurar la correcta serialización del JSON.
        /// </remarks>
        public async Task<bool> TieneReservasEnDescanso(int estilistaId, DayOfWeek dia, TimeSpan inicio, TimeSpan fin)
        {
            try
            {
                // Convertimos TimeSpan (Monolito) a TimeOnly (Microservicio)
                var body = new
                {
                    EstilistaId = estilistaId,
                    DiaSemana = dia,
                    HoraInicio = TimeOnly.FromTimeSpan(inicio),
                    HoraFin = TimeOnly.FromTimeSpan(fin)
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/validaciones/descanso", content);

                if (!response.IsSuccessStatusCode) return true;

                var result = await response.Content.ReadAsStringAsync();
                return bool.Parse(result);
            }
            catch
            {
                return true;
            }
        }
    }
}