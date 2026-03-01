using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using frontend.Models;
using Newtonsoft.Json;

namespace frontend.Services
{
    public static class ApiService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:7272/") // adres Twojego API
        };

        public static async Task<LoginResponse> LoginAsync(string email, string password)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new LoginRequest { Email = email, Password = password };
            string json = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return loginResponse ?? new LoginResponse { IsAuthenticated = false };
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new LoginResponse { IsAuthenticated = false };
            }
            else
            {
                throw new Exception($"Błąd serwera: {response.StatusCode}");
            }
        }
    }
}