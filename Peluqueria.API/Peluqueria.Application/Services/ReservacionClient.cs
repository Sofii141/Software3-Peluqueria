using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Infrastructure.External
{
    public class ReservacionClient : IReservacionClient
    {
        private readonly HttpClient _httpClient;

        public ReservacionClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<bool> GetBoolAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return true;

                var content = await response.Content.ReadAsStringAsync();
                return bool.Parse(content);
            }
            catch
            {
                return true;
            }
        }

        public async Task<bool> TieneReservasEstilista(int id)
            => await GetBoolAsync($"api/validaciones/estilista/{id}");

        public async Task<bool> TieneReservasServicio(int id)
            => await GetBoolAsync($"api/validaciones/servicio/{id}");

        public async Task<bool> TieneReservasCategoria(int id)
            => await GetBoolAsync($"api/validaciones/categoria/{id}");

        // BLINDADO con System.DayOfWeek
        public async Task<bool> TieneReservasEnDia(int id, System.DayOfWeek dia)
            => await GetBoolAsync($"api/validaciones/estilista/{id}/dia-semana/{(int)dia}");

        // BLINDADO con System.DateOnly
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

        // --- MÉTODO BLINDADO (SOLUCIÓN DEL ERROR) ---
        public async Task<bool> TieneReservasEnDescanso(int estilistaId, System.DayOfWeek dia, System.TimeSpan inicio, System.TimeSpan fin)
        {
            try
            {
                var body = new
                {
                    EstilistaId = estilistaId,
                    DiaSemana = dia,
                    // Convertimos explícitamente de System.TimeSpan a TimeOnly
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