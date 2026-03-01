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

            public int? KlientId { get; set; }
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

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string email = txtUsername.Text;
            string password = txtPassword.Password;

            try
            {
                var result = await LoginApiAsync(email, password);

                if (result != null && result.IsAuthenticated)
                {
                    switch (result.Role)
                    {
                        case "kurier":
                            new CourierPanel().Show();
                            break;
                        case "klient":
                            MainWindow main = new MainWindow(result.KlientId);
                            main.Show();
                            break;
                        default:
                            MessageBox.Show("Nieznana rola użytkownika.");
                            return;
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Błędne dane logowania!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                UserCredential credential;
                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { "profile", "email" },
                        "user",
                        CancellationToken.None
                    );
                }

                if (credential != null && !string.IsNullOrEmpty(credential.Token.AccessToken))
                {
                    CourierPanel panel = new CourierPanel();
                    panel.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas logowania: " + ex.Message);
            }
        }
    }
}