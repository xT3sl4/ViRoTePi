using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace frontend
{
    public partial class MainWindow : Window
    {
        private int? klientId; 

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(int? klientId)
        {
            InitializeComponent();
            this.klientId = klientId;
               
            Loaded += async (s, e) =>
            {
                List<Pack> packs = await GetPacksAsync(this.klientId);
                PacksDataGrid.ItemsSource = packs;
            };
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
            public string name { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
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

        #endregion

        #region Menu i panele

        public async void Packages_Click(object sender, RoutedEventArgs e)
        {
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

        private void OtworzMape()
        {
            CustomMessageBox mapaWindow = new CustomMessageBox { Owner = this };
            if (mapaWindow.ShowDialog() == true)
            {
                var wybrany = mapaWindow.WybranyPaczkomat;
                if (wybrany != null)
                {
                    SelectedLockerInfo.Text = $"Wybrany punkt: {wybrany.name}";
                    SelectedLockerInfo.FontStyle = FontStyles.Normal;
                    SelectedLockerInfo.FontWeight = FontWeights.Bold;
                    SendPackage_Click(null, null);
                }
            }
        }

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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Imie: " + Firstname_TextBox.Text + "\n" +
                            "Nazwisko: " + Surname_TextBox.Text + "\n" +
                            "Email: " + EmailAdres_TextBox.Text + "\n" +
                            "Numer Telefonu: " + PhoneValueText.Text + "\n" +
                            SelectedLockerInfo.Text);
        }

        #endregion
    }
}