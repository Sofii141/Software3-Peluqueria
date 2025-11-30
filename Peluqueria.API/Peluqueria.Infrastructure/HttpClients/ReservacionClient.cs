using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Peluqueria.Application.Interfaces;

namespace Peluqueria.Infrastructure.HttpClients
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

        public async Task<bool> TieneReservasEnDia(int id, DayOfWeek dia)
            => await GetBoolAsync($"api/validaciones/estilista/{id}/dia-semana/{(int)dia}");

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