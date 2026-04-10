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
        private string _selectedPaczkomatName = null;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureUIForGuest();
        }

        public MainWindow(int? klientId)
        {
            InitializeComponent();
            this.klientId = klientId;

            if (klientId.HasValue)
            {
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

        private void ConfigureUIForGuest()
        {
            PackagesPanel.Visibility = Visibility.Collapsed;
            SendPackagePanel.Visibility = Visibility.Visible;
        }

        #region Modele

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
        #endregion

        #region HttpClient

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
                    HttpResponseMessage response = await client.GetAsync($"api/auth/check-email?email={Uri.EscapeDataString(email)}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<CheckResponse>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return result?.Exists ?? false;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        private async Task<bool> CheckPhoneExistsAsync(string phone)
        {
            using (HttpClient client = CreateHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync($"api/auth/check-phone?phone={Uri.EscapeDataString(phone)}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<CheckResponse>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return result?.Exists ?? false;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region Menu i panele

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

        public void User_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginWindow = new LoginPage();
            loginWindow.Show();
            Window.GetWindow((DependencyObject)sender)?.Close();
        }

        #endregion

        #region Wybór paczki i slider

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

        #endregion

        #region Wysyłanie paczki

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
                    else
                    {
                        return;
                    }
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
                    MessageBox.Show("Paczka została wysłana pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
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
        #endregion
    }
}
