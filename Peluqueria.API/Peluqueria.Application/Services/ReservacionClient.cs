using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Infrastructure.External
{
    /// <summary>
    /// Cliente HTTP encargado de la comunicación sincrónica con el Microservicio de Reservas.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa una estrategia de **"Fail-Safe" (Fallo Seguro)**:
    /// Si la comunicación con el microservicio falla (TimeOut, 500, Red caída), los métodos retornan <c>true</c>.
    /// <br/>
    /// <b>¿Por qué?</b> 
    /// Es más seguro bloquear una operación (ej. impedir borrar un estilista) erróneamente, 
    /// que permitir borrarlo y dejar citas huérfanas en el otro sistema.
    /// </remarks>
    public class ReservacionClient : IReservacionClient
    {
        private readonly HttpClient _httpClient;

        public ReservacionClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Método auxiliar para ejecutar peticiones GET y procesar la respuesta booleana.
        /// </summary>
        /// <param name="url">Endpoint relativo a consultar.</param>
        /// <returns>El valor de la API o <c>true</c> si ocurre cualquier excepción.</returns>
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
                // Loguear error aquí sería ideal en un entorno productivo
                return true; // Fail-Safe
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
        public async Task<bool> TieneReservasEnDia(int id, System.DayOfWeek dia)
            => await GetBoolAsync($"api/validaciones/estilista/{id}/dia-semana/{(int)dia}");

        /// <summary>
        /// Valida si hay reservas en un rango de fechas calendario.
        /// Útil antes de aprobar unas vacaciones (Bloqueo).
        /// </summary>
        public async Task<bool> TieneReservasEnRango(int id, System.DateOnly inicio, System.DateOnly fin)
        {
            try
            {
                var body = new { EstilistaId = id, FechaInicio = inicio, FechaFin = fin };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

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
        /// Valida si hay reservas en una franja horaria específica de un día.
        /// Útil antes de asignar una hora de almuerzo o descanso fijo.
        /// </summary>
        /// <remarks>
        /// Convierte los tipos <see cref="TimeSpan"/> del Monolito a <see cref="TimeOnly"/> para el Microservicio.
        /// </remarks>
        public async Task<bool> TieneReservasEnDescanso(int estilistaId, System.DayOfWeek dia, System.TimeSpan inicio, System.TimeSpan fin)
        {
            try
            {
                var body = new
                {
                    EstilistaId = estilistaId,
                    DiaSemana = dia,
                    // Conversión explícita necesaria: .NET 8 JSON serializer prefiere TimeOnly para horas
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