using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace frontend
{
    public partial class UserEditDialog : Window
    {
        // Jeśli EditUserId > 0 to tryb edycji, inaczej dodawanie
        public int EditUserId { get; private set; } = 0;

        public UserEditDialog()
        {
            InitializeComponent();
            DialogTitle.Text = "Nowy użytkownik";
            SaveBtn.Content = "Dodaj";
            // W trybie dodawania hasło jest wymagane
        }

        public UserEditDialog(AdminUserDto user)
        {
            InitializeComponent();
            EditUserId = user.Id;
            DialogTitle.Text = "Edytuj użytkownika";
            SaveBtn.Content = "Zapisz zmiany";

            // Wypełnij formularz danymi użytkownika
            NameBox.Text = user.Name;
            SurnameBox.Text = user.Surname;
            EmailBox.Text = user.Email;
            PhoneBox.Text = user.PhoneNumber?.ToString() ?? "";

            // Ustaw odpowiedni element ComboBox
            foreach (ComboBoxItem item in RoleComboBox.Items)
            {
                if (item.Tag?.ToString() == user.Role)
                {
                    RoleComboBox.SelectedItem = item;
                    break;
                }
            }
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

            string name = NameBox.Text.Trim();
            string surname = SurnameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;
            string phone = PhoneBox.Text.Trim();
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            // Walidacja
            if (string.IsNullOrWhiteSpace(name)) { ErrorText.Text = "Podaj imię."; NameBox.Focus(); return; }
            if (string.IsNullOrWhiteSpace(surname)) { ErrorText.Text = "Podaj nazwisko."; SurnameBox.Focus(); return; }
            if (string.IsNullOrWhiteSpace(email)) { ErrorText.Text = "Podaj email."; EmailBox.Focus(); return; }
            if (EditUserId == 0 && string.IsNullOrWhiteSpace(password)) { ErrorText.Text = "Podaj hasło."; PasswordBox.Focus(); return; }
            if (string.IsNullOrWhiteSpace(role)) { ErrorText.Text = "Wybierz rolę."; RoleComboBox.Focus(); return; }

            SaveBtn.IsEnabled = false;
            SaveBtn.Content = "Zapisywanie...";

            try
            {
                using (var client = CreateHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response;

                    if (EditUserId == 0)
                    {
                        // Tryb dodawania
                        var request = new
                        {
                            Name = name,
                            Surname = surname,
                            Email = email,
                            Password = password,
                            Role = role,
                            PhoneNumber = phone
                        };
                        string json = JsonSerializer.Serialize(request);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PostAsync("api/users", content);
                    }
                    else
                    {
                        // Tryb edycji
                        var request = new
                        {
                            Name = name,
                            Surname = surname,
                            Email = email,
                            Password = password, // Puste hasło = brak zmiany (obsłużone w backendzie)
                            Role = role,
                            PhoneNumber = phone
                        };
                        string json = JsonSerializer.Serialize(request);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PutAsync($"api/users/{EditUserId}", content);
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        try
                        {
                            var error = JsonDocument.Parse(responseBody);
                            ErrorText.Text = error.RootElement.GetProperty("message").GetString() ?? responseBody;
                        }
                        catch
                        {
                            ErrorText.Text = "Błąd: " + responseBody;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Błąd połączenia: " + ex.Message;
            }
            finally
            {
                SaveBtn.IsEnabled = true;
                SaveBtn.Content = EditUserId == 0 ? "Dodaj" : "Zapisz zmiany";
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
