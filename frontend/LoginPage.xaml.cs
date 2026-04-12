using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace frontend
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class LoginResponse
        {
            public bool IsAuthenticated { get; set; }
            public string Role { get; set; }
            public int? UserId { get; set; }
            public int? KlientId { get; set; }
            public int? KurierId { get; set; }
            public string Selfie { get; set; }
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7272/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<LoginResponse> LoginApiAsync(string email, string password)
        {
            using (var client = CreateHttpClient())
            {
                var request = new LoginRequest { Email = email, Password = password };
                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<LoginResponse>(
                        responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new LoginResponse { IsAuthenticated = false };
                }
                else
                {
                    MessageBox.Show($"Błąd serwera: {response.StatusCode}");
                    return new LoginResponse { IsAuthenticated = false };
                }
            }
        }

        private void NavigateToPanel(LoginResponse result)
        {
            switch (result.Role)
            {
                case "kurier":
                    new CourierPanel(result.KurierId, result.Selfie).Show();
                    break;
                case "klient":
                    new MainWindow(result.KlientId, result.Selfie, result.UserId).Show();
                    break;
                case "admin":
                    new AdminPanel(result.Selfie).Show();
                    break;
                default:
                    MessageBox.Show("Nieznana rola użytkownika.");
                    return;
            }
            this.Close();
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string email = txtUsername.Text;
            string password = txtPassword.Password;

            try
            {
                var result = await LoginApiAsync(email, password);

                if (result != null && result.IsAuthenticated)
                    NavigateToPanel(result);
                else
                    MessageBox.Show("Błędne dane logowania!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia z API: " + ex.Message);
            }
        }

        private async void LoginGoogle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string credentialPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "frontend_google_auth"
                );

                // Usuń zapisany token — dzięki temu zawsze pojawia się wybór konta.
                // Usunięcie cache + JEDNO AuthorizeAsync = dokładnie jedno okno przeglądarki.
                if (Directory.Exists(credentialPath))
                    Directory.Delete(credentialPath, recursive: true);

                UserCredential credential;
                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { "profile", "email" },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credentialPath, fullPath: true)
                    );
                }

                if (credential == null || string.IsNullOrEmpty(credential.Token.AccessToken))
                {
                    MessageBox.Show("Nie udało się pobrać tokena Google.");
                    return;
                }

                // Pobierz dane użytkownika z Google (email, imię, nazwisko)
                var googleUser = await GetGoogleUserInfoAsync(credential.Token.AccessToken);
                if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
                {
                    MessageBox.Show("Nie udało się pobrać adresu email z konta Google.");
                    return;
                }

                // Sprawdź czy konto istnieje w systemie
                var result = await LoginGoogleApiAsync(googleUser.Email);

                if (result != null && result.IsAuthenticated)
                {
                    // Konto istnieje — zaloguj
                    NavigateToPanel(result);
                }
                else
                {
                    // Brak konta — otwórz rejestrację z danymi z Google już wypełnionymi
                    var registerWindow = new RegisterWindow(
                        email: googleUser.Email,
                        firstName: googleUser.GivenName ?? "",
                        lastName: googleUser.FamilyName ?? ""
                    );
                    registerWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas logowania: " + ex.Message);
            }
        }

        private async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await http.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (!response.IsSuccessStatusCode)
                    return null;

                string raw = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GoogleUserInfo>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }

        private async Task<LoginResponse> LoginGoogleApiAsync(string email)
        {
            using (var client = CreateHttpClient())
            {
                var request = new { Email = email };
                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("api/auth/login-google", content);
                string rawResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<LoginResponse>(rawResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new LoginResponse { IsAuthenticated = false };
                }

                MessageBox.Show($"Błąd serwera: {response.StatusCode}");
                return new LoginResponse { IsAuthenticated = false };
            }
        }

        private class GoogleUserInfo
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}