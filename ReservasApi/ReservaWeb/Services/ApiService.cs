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
        // --- CRUD CATEGORÍAS ---

        public async Task<List<CategoriaViewModel>> GetCategoriasAsync()
        {
            var response = await _httpClient.GetAsync(_baseUrl + "Categorias");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CategoriaViewModel>>(content) ?? new List<CategoriaViewModel>();
            }
            return new List<CategoriaViewModel>();
        }

        public async Task<CategoriaViewModel?> GetCategoriaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"Categorias/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CategoriaViewModel>(content);
            }
            return null;
        }

        public async Task<bool> CreateCategoriaAsync(CategoriaViewModel categoria, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(categoria), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl + "Categorias", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCategoriaAsync(int id, CategoriaViewModel categoria, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(categoria), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_baseUrl + $"Categorias/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCategoriaAsync(int id, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync(_baseUrl + $"Categorias/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- CRUD BEBIDAS ---

        public async Task<List<BebidaViewModel>> GetBebidasAsync()
        {
            var response = await _httpClient.GetAsync(_baseUrl + "Bebidas");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BebidaViewModel>>(content) ?? new List<BebidaViewModel>();
            }
            return new List<BebidaViewModel>();
        }

        public async Task<BebidaViewModel?> GetBebidaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"Bebidas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BebidaViewModel>(content);
            }
            return null;
        }

        public async Task<bool> CreateBebidaAsync(BebidaViewModel bebida, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(bebida), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl + "Bebidas", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateBebidaAsync(int id, BebidaViewModel bebida, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(bebida), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_baseUrl + $"Bebidas/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBebidaAsync(int id, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync(_baseUrl + $"Bebidas/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}