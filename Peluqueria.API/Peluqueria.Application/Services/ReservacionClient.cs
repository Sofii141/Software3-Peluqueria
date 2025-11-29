using System.Text;
using System.Text.Json;
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

        public async Task<bool> TieneReservasEnDia(int id, DayOfWeek dia)
            => await GetBoolAsync($"api/validaciones/estilista/{id}/dia/{(int)dia}");

        public async Task<bool> TieneReservasEnRango(int id, DateOnly inicio, DateOnly fin)
        {
            try
            {
                var body = new { EstilistaId = id, FechaInicio = inicio, FechaFin = fin };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/validaciones/estilista/rango", content);
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