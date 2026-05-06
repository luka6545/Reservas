using Newtonsoft.Json;
using ReservaWeb.Models;
using System.Net.Http.Headers;
using System.Text;

namespace ReservaWeb.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiSettings:BaseUrl"]!;
        }

        // Método para Iniciar Sesión
        public async Task<string?> LoginAsync(string email, string password)
        {
            var loginData = new { Email = email, Password = password };
            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl + "Auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(result)!;
                return json.token; // Retorna el JWT
            }
            return null;
        }

        public async Task<bool> RegistrarAsync(string nombre, string email, string password)
        {
            var registroData = new { Nombre = nombre, Email = email, Password = password };
            var content = new StringContent(JsonConvert.SerializeObject(registroData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl + "Auth/registro", content);
            return response.IsSuccessStatusCode;
        }
        //GET ZONAS
        public async Task<List<ZonaViewModel>> GetZonasAsync(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(_baseUrl + "Zonas");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ZonaViewModel>>(content) ?? new List<ZonaViewModel>();
            }
            return new List<ZonaViewModel>();
        }
        // GET: Una zona por ID (Para editar)
        public async Task<ZonaViewModel?> GetZonaByIdAsync(int id, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(_baseUrl + $"Zonas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ZonaViewModel>(content);
            }
            return null;
        }

        // POST: Crear zona
        public async Task<bool> CreateZonaAsync(ZonaViewModel zona, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(zona), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl + "Zonas", content);
            return response.IsSuccessStatusCode;
        }

        // PUT: Editar zona
        public async Task<bool> UpdateZonaAsync(int id, ZonaViewModel zona, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(zona), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_baseUrl + $"Zonas/{id}", content);
            return response.IsSuccessStatusCode;
        }

        // DELETE: Eliminar zona
        public async Task<bool> DeleteZonaAsync(int id, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync(_baseUrl + $"Zonas/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}