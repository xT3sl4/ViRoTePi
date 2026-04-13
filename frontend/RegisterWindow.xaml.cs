using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace frontend
{
    public partial class RegisterWindow : Window
    {
        private string _prefilledEmail;


        public RegisterWindow()
        {
            InitializeComponent();
        }


        public RegisterWindow(string email)
        {
            InitializeComponent();
            _prefilledEmail = email;

            if (!string.IsNullOrWhiteSpace(email))
            {
                EmailBox.Text = email;
                NameBox.Focus();
            }
        }

        public RegisterWindow(string email, string firstName, string lastName)
        {
            InitializeComponent();
            _prefilledEmail = email;

            if (!string.IsNullOrWhiteSpace(email))
                EmailBox.Text = email;

            if (!string.IsNullOrWhiteSpace(firstName))
                NameBox.Text = firstName;

            if (!string.IsNullOrWhiteSpace(lastName))
                SurnameBox.Text = lastName;


            if (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(firstName))
            {
                GoogleInfoBanner.Visibility = Visibility.Visible;
                GoogleInfoText.Text = $"Dane pobrane z Google ({email})";
            }

            PasswordBox.Focus();
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7272/")
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                ErrorText.Text = "Podaj imię.";
                NameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(SurnameBox.Text))
            {
                ErrorText.Text = "Podaj nazwisko.";
                SurnameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                ErrorText.Text = "Podaj adres email.";
                EmailBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ErrorText.Text = "Podaj hasło.";
                PasswordBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                ErrorText.Text = "Podaj numer telefonu.";
                PhoneBox.Focus();
                return;
            }
            if (!IsValidEmail(EmailBox.Text.Trim()))
            {
                ErrorText.Text = "Podaj prawidłowy adres email.";
                EmailBox.Focus();
                return;
            }

            string phoneClean = PhoneBox.Text.Replace(" ", "").Replace("-", "").Trim();
            if (!long.TryParse(phoneClean, out _))
            {
                ErrorText.Text = "Numer telefonu może zawierać tylko cyfry.";
                PhoneBox.Focus();
                return;
            }
            if (phoneClean.Length < 9 || phoneClean.Length > 11)
            {
                ErrorText.Text = "Numer telefonu musi mieć 9-11 cyfr.";
                PhoneBox.Focus();
                return;
            }

            var request = new
            {
                Name = NameBox.Text.Trim(),
                Surname = SurnameBox.Text.Trim(),
                Email = EmailBox.Text.Trim(),
                Password = PasswordBox.Password,
                PhoneNumber = phoneClean
            };

            using (var client = CreateHttpClient())
            {
                try
                {
                    string json = JsonSerializer.Serialize(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/auth/register", content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            "Konto zostało utworzone pomyślnie!\n\nMożesz się teraz zalogować.",
                            "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoginPage loginPage = new LoginPage();
                        loginPage.Show();
                        this.Close();
                    }
                    else
                    {
                        try
                        {
                            var error = JsonDocument.Parse(responseBody);
                            if (error.RootElement.TryGetProperty("message", out var messageProperty))
                                ErrorText.Text = messageProperty.GetString();
                            else
                                ErrorText.Text = "Błąd rejestracji: " + responseBody;
                        }
                        catch
                        {
                            ErrorText.Text = "Błąd rejestracji: " + responseBody;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorText.Text = "Błąd połączenia z serwerem: " + ex.Message;
                }
            }
        }


        private async void RegisterGoogle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string credentialPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "frontend_google_auth"
                );


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
                    ErrorText.Text = "Nie udało się pobrać tokena Google.";
                    return;
                }

                string googleJson = await GetGoogleUserInfoAsync(credential.Token.AccessToken);
                if (googleJson == null)
                {
                    ErrorText.Text = "Nie udało się pobrać danych z Google.";
                    return;
                }

                var googleUser = JsonSerializer.Deserialize<GoogleUserInfo>(googleJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                if (!string.IsNullOrWhiteSpace(googleUser?.Email))
                    EmailBox.Text = googleUser.Email;

                if (!string.IsNullOrWhiteSpace(googleUser?.GivenName))
                    NameBox.Text = googleUser.GivenName;
                else if (!string.IsNullOrWhiteSpace(googleUser?.Name))
                    NameBox.Text = googleUser.Name.Split(' ')[0];

                if (!string.IsNullOrWhiteSpace(googleUser?.FamilyName))
                    SurnameBox.Text = googleUser.FamilyName;

                GoogleInfoBanner.Visibility = Visibility.Visible;
                GoogleInfoText.Text = $"Dane pobrane z Google ({googleUser?.Email})";

                PasswordBox.Focus();
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Błąd logowania Google: " + ex.Message;
            }
        }

        private async Task<string> GetGoogleUserInfoAsync(string accessToken)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await http.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsStringAsync();
            }
        }

        private class GoogleUserInfo
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void User_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginWindow = new LoginPage();
            loginWindow.Show();
            this.Close();
        }
    }
}