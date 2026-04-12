using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using frontend;

namespace frontend
{
    public partial class MainWindow : Window
    {
        private int? klientId;
        private int? _userId;
        private string _selectedPaczkomatName = null;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureUIForGuest();
        }

        public MainWindow(int? klientId, string selfie = null, int? userId = null)
        {
            InitializeComponent();
            this.klientId = klientId;
            this._userId = userId;

            if (klientId.HasValue)
            {
                // Zalogowany — pokaż selfie i przycisk wyloguj
                ConfigureUIForLoggedIn(selfie, userId);

                Loaded += async (s, e) =>
                {
                    List<Pack> packs = await GetPacksAsync(this.klientId);
                    PacksDataGrid.ItemsSource = packs;
                };
            }
            else
            {
                ConfigureUIForGuest();
            }
        }

        // ── UI helpers ────────────────────────────────────────────────────────

        private void ConfigureUIForLoggedIn(string selfieFileName, int? userId)
        {
            // Pokaż panel zalogowanego, ukryj przycisk "Zaloguj się"
            LoggedInPanel.Visibility = Visibility.Visible;
            LoginBtn.Visibility = Visibility.Collapsed;

            // Pobierz i ustaw selfie
            LoadSelfie(selfieFileName);

            // Pokaż imię jeśli uda się pobrać (async — nie blokujemy UI)
            if (userId.HasValue)
                _ = LoadUserNameAsync(userId.Value);

            // Pokaż listę paczek jako domyślny widok
            PackagesPanel.Visibility = Visibility.Visible;
            SendPackagePanel.Visibility = Visibility.Collapsed;
        }

        private void ConfigureUIForGuest()
        {
            // Gość — ukryj selfie i wyloguj, pokaż "Zaloguj się"
            LoggedInPanel.Visibility = Visibility.Collapsed;
            LoginBtn.Visibility = Visibility.Visible;
            SelfieEllipse.Visibility = Visibility.Collapsed;
            DefaultUserIcon.Visibility = Visibility.Visible;

            PackagesPanel.Visibility = Visibility.Collapsed;
            SendPackagePanel.Visibility = Visibility.Visible;
        }

        private void LoadSelfie(string selfieFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selfieFileName))
                {
                    // Brak selfie — zostaje domyślna ikona
                    SelfieEllipse.Visibility = Visibility.Collapsed;
                    DefaultUserIcon.Visibility = Visibility.Visible;
                    return;
                }

                string path = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Images", selfieFileName);

                if (!System.IO.File.Exists(path))
                {
                    SelfieEllipse.Visibility = Visibility.Collapsed;
                    DefaultUserIcon.Visibility = Visibility.Visible;
                    return;
                }

                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ProfileImageBrush.ImageSource = bitmap;
                SelfieEllipse.Visibility = Visibility.Visible;
                DefaultUserIcon.Visibility = Visibility.Collapsed;
            }
            catch
            {
                SelfieEllipse.Visibility = Visibility.Collapsed;
                DefaultUserIcon.Visibility = Visibility.Visible;
            }
        }

        private async Task LoadUserNameAsync(int userId)
        {
            try
            {
                using (var client = CreateHttpClient())
                {
                    var response = await client.GetAsync($"api/users/{userId}");
                    if (!response.IsSuccessStatusCode) return;

                    string json = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserProfile>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (user != null)
                        UserNameText.Text = $"{user.Name} {user.Surname}";
                }
            }
            catch { }
        }

        // ── Modele ────────────────────────────────────────────────────────────

        public class Pack
        {
            public int PackId { get; set; }
            public string Size { get; set; }
            public string Delivered { get; set; } = "";
            public string Date { get; set; }
            public string KlientName { get; set; } = "";
        }

        public class PackResponse
        {
            [JsonPropertyName("$values")]
            public List<Pack> Values { get; set; } = new List<Pack>();
        }

        public class PaczkomatMapaData
        {
            public int id { get; set; }
            public string name { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class CheckResponse
        {
            public bool Exists { get; set; }
        }

        public class UserProfile
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public int? PhoneNumber { get; set; }
            public string Role { get; set; }
        }

        // ── HttpClient ────────────────────────────────────────────────────────

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7272/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<List<Pack>> GetPacksAsync(int? klientId = null)
        {
            using (HttpClient client = CreateHttpClient())
            {
                string url = $"api/packs?klientId={klientId}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Błąd pobierania paczek: {response.StatusCode}");
                    return new List<Pack>();
                }

                string json = await response.Content.ReadAsStringAsync();
                PackResponse wrapper = JsonSerializer.Deserialize<PackResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapper?.Values ?? new List<Pack>();
            }
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            using (HttpClient client = CreateHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(
                        $"api/auth/check-email?email={Uri.EscapeDataString(email)}");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<CheckResponse>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return result?.Exists ?? false;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        private async Task<bool> CheckPhoneExistsAsync(string phone)
        {
            using (HttpClient client = CreateHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(
                        $"api/auth/check-phone?phone={Uri.EscapeDataString(phone)}");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<CheckResponse>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return result?.Exists ?? false;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        // ── Nawigacja ─────────────────────────────────────────────────────────

        public async void Packages_Click(object sender, RoutedEventArgs e)
        {
            if (!klientId.HasValue)
            {
                MessageBox.Show("Musisz być zalogowany, aby zobaczyć swoje paczki.",
                    "Wymagane logowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PackagesPanel.Visibility = Visibility.Visible;
            SendPackagePanel.Visibility = Visibility.Collapsed;

            List<Pack> packs = await GetPacksAsync(klientId);
            PacksDataGrid.ItemsSource = packs;
        }

        public void SendPackage_Click(object sender, RoutedEventArgs e)
        {
            PackagesPanel.Visibility = Visibility.Collapsed;
            SendPackagePanel.Visibility = Visibility.Visible;
        }

        private void MapButton_Click(object sender, RoutedEventArgs e) => OtworzMape();
        private void LockerButton_Click(object sender, RoutedEventArgs e) => OtworzMape();

        // Kliknięcie na zdjęcie/ikonę — edycja profilu (tylko gdy zalogowany)
        public void User_Click(object sender, RoutedEventArgs e)
        {
            if (_userId.HasValue)
            {
                OpenProfileEdit();
            }
            else
            {
                // Gość — wróć do logowania
                LoginPage loginWindow = new LoginPage();
                loginWindow.Show();
                this.Close();
            }
        }

        // Wylogowanie
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Czy na pewno chcesz się wylogować?",
                "Wylogowanie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            LoginPage loginPage = new LoginPage();
            loginPage.Show();
            this.Close();
        }

        private async void OpenProfileEdit()
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"api/users/{_userId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Nie udało się pobrać danych profilu.", "Błąd",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserProfile>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (user == null) return;

                    var editWindow = new ProfileEditWindow(
                        userId: _userId.Value,
                        name: user.Name ?? "",
                        surname: user.Surname ?? "",
                        email: user.Email ?? "",
                        phone: user.PhoneNumber?.ToString() ?? ""
                    );
                    editWindow.Owner = this;
                    editWindow.ProfileUpdated += () =>
                    {
                        // Odśwież imię w nagłówku po zapisie
                        _ = LoadUserNameAsync(_userId.Value);
                    };
                    editWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ── Slider i rozmiary paczki ──────────────────────────────────────────

        private void small_parcel_Selected(object sender, RoutedEventArgs e) => price_to_pay.Text = "Do zapłaty: 14.99 zł";
        private void medium_parcel_Selected(object sender, RoutedEventArgs e) => price_to_pay.Text = "Do zapłaty: 16.99 zł";
        private void big_parcel_Selected(object sender, RoutedEventArgs e) => price_to_pay.Text = "Do zapłaty: 19.99 zł";

        private void PhoneSlider_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L) PhoneSlider.Value = Math.Min(PhoneSlider.Maximum, PhoneSlider.Value + 100);
            else if (e.Key == Key.J) PhoneSlider.Value = Math.Max(PhoneSlider.Minimum, PhoneSlider.Value - 100);
        }

        private void PhoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PhoneValueText != null)
                PhoneValueText.Text = ((long)e.NewValue).ToString("000 000 000");
        }

        // ── Wysyłanie paczki ──────────────────────────────────────────────────

        public class SendPackRequest
        {
            public string ReceiverEmail { get; set; }
            public string ReceiverPhone { get; set; }
            public string Size { get; set; }
            public string PaczkomatName { get; set; }
            public int SenderKlientId { get; set; }
        }

        private int? _selectedPaczkomatId = null;

        private void OtworzMape()
        {
            CustomMessageBox mapaWindow = new CustomMessageBox { Owner = this };
            if (mapaWindow.ShowDialog() == true)
            {
                var wybrany = mapaWindow.WybranyPaczkomat;
                if (wybrany != null)
                {
                    _selectedPaczkomatName = wybrany.name;
                    SelectedLockerInfo.Text = $"Wybrany punkt: {wybrany.name}";
                    SelectedLockerInfo.FontStyle = FontStyles.Normal;
                    SelectedLockerInfo.FontWeight = FontWeights.Bold;
                    SendPackage_Click(null, null);
                }
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string senderEmail = EmailAdres_TextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                MessageBox.Show("Podaj email nadawcy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string receiverPhone = PhoneValueText.Text.Replace(" ", "").Trim();
            if (string.IsNullOrWhiteSpace(receiverPhone))
            {
                MessageBox.Show("Podaj numer telefonu odbiorcy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_selectedPaczkomatName))
            {
                MessageBox.Show("Wybierz paczkomat z mapy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string size = null;
            if (small_parcel.IsSelected) size = "S";
            else if (medium_parcel.IsSelected) size = "M";
            else if (big_parcel.IsSelected) size = "L";

            if (size == null)
            {
                MessageBox.Show("Wybierz rozmiar paczki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool receiverExists = await CheckPhoneExistsAsync(receiverPhone);
            if (!receiverExists)
            {
                MessageBox.Show("Brak konta powiązanego z tym numerem telefonu odbiorcy.\n\n" +
                    "Odbiorca musi posiadać konto w systemie, aby otrzymać paczkę.",
                    "Odbiorca nieznaleziony", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!klientId.HasValue)
            {
                bool senderExists = await CheckEmailExistsAsync(senderEmail);
                if (!senderExists)
                {
                    var result = MessageBox.Show(
                        $"Nie znaleziono konta powiązanego z adresem email: {senderEmail}\n\n" +
                        "Czy chcesz utworzyć nowe konto?",
                        "Brak konta nadawcy",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        RegisterWindow registerWindow = new RegisterWindow(senderEmail);
                        registerWindow.ShowDialog();

                        bool nowExists = await CheckEmailExistsAsync(senderEmail);
                        if (!nowExists)
                        {
                            MessageBox.Show("Rejestracja nie została ukończona. Spróbuj ponownie.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    else return;
                }
            }

            var request = new SendPackRequest
            {
                ReceiverEmail = senderEmail,
                ReceiverPhone = receiverPhone,
                Size = size,
                PaczkomatName = _selectedPaczkomatName
            };

            using (var client = CreateHttpClient())
            {
                string json = System.Text.Json.JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/packs/send", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Paczka została wysłana pomyślnie!", "Sukces",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    EmailAdres_TextBox.Text = "";
                    PhoneValueText.Text = "500 000 000";
                    PhoneSlider.Value = 500000000;
                    SelectedLockerInfo.Text = "Nie wybrano paczkomatu";
                    SelectedLockerInfo.FontStyle = FontStyles.Italic;
                    SelectedLockerInfo.FontWeight = FontWeights.Normal;
                    _selectedPaczkomatName = null;
                }
                else
                {
                    try
                    {
                        var error = System.Text.Json.JsonDocument.Parse(responseBody);
                        string msg = error.RootElement.GetProperty("message").GetString() ?? responseBody;
                        MessageBox.Show(msg, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch
                    {
                        MessageBox.Show($"Błąd: {responseBody}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}