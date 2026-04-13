using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace frontend
{
    public partial class ProfileEditWindow : Window
    {
        private readonly int _userId;

        // Wywoływane po udanym zapisie — można odświeżyć dane w oknie nadrzędnym
        public event Action ProfileUpdated;

        public ProfileEditWindow(int userId, string name, string surname, string email, string phone)
        {
            InitializeComponent();
            _userId = userId;

            NameBox.Text = name ?? "";
            SurnameBox.Text = surname ?? "";
            EmailBox.Text = email ?? "";
            PhoneBox.Text = phone ?? "";
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7272/")
            };
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";
            SuccessText.Text = "";

            string name = NameBox.Text.Trim();
            string surname = SurnameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string password = PasswordBox.Password;

            // Walidacja
            if (string.IsNullOrWhiteSpace(name))
            {
                ErrorText.Text = "Imię nie może być puste.";
                NameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(surname))
            {
                ErrorText.Text = "Nazwisko nie może być puste.";
                SurnameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                ErrorText.Text = "Podaj prawidłowy adres e-mail.";
                EmailBox.Focus();
                return;
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                string cleanPhone = phone.Replace(" ", "").Replace("-", "");
                if (!long.TryParse(cleanPhone, out _) || cleanPhone.Length < 9 || cleanPhone.Length > 11)
                {
                    ErrorText.Text = "Numer telefonu musi mieć 9–11 cyfr.";
                    PhoneBox.Focus();
                    return;
                }
            }

            SaveBtn.IsEnabled = false;
            SaveBtn.Content = "Zapisywanie...";

            try
            {
                var requestBody = new
                {
                    Name = name,
                    Surname = surname,
                    Email = email,
                    Password = password,   // puste = bez zmiany (obsługa w backendzie)
                    PhoneNumber = phone
                    // Role nie wysyłamy — klient nie może zmieniać własnej roli
                };

                string json = JsonSerializer.Serialize(requestBody);

                using (var client = CreateHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"api/users/{_userId}", content);
                    string body = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        SuccessText.Text = "Profil został zaktualizowany!";
                        ProfileUpdated?.Invoke();
                    }
                    else
                    {
                        try
                        {
                            var err = JsonDocument.Parse(body);
                            ErrorText.Text = err.RootElement.GetProperty("message").GetString() ?? body;
                        }
                        catch
                        {
                            ErrorText.Text = "Błąd: " + body;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Błąd połączenia z serwerem: " + ex.Message;
            }
            finally
            {
                SaveBtn.IsEnabled = true;
                SaveBtn.Content = "Zapisz zmiany";
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}