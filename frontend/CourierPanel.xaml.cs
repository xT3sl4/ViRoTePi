using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace frontend
{
    public class Parcel
    {
        public int PackId { get; set; }
        public string OrderNumber { get; set; }
        public string LockerCode { get; set; }
        public string Size { get; set; }
        public string Delivered { get; set; }
        public string Date { get; set; }
        public string KlientName { get; set; }
    }

    public class ParcelResponse
    {
        [JsonPropertyName("$values")]
        public List<Parcel> Values { get; set; } = new List<Parcel>();
    }

    public partial class CourierPanel : Window
    {
        public ObservableCollection<Parcel> ActiveParcels { get; set; }
        private int? _kurierId;

        public CourierPanel(int? kurierId = null)
        {
            InitializeComponent();
            _kurierId = kurierId;
            ActiveParcels = new ObservableCollection<Parcel>();
            PackagesList.ItemsSource = ActiveParcels;

            Loaded += async (s, e) => await LoadParcels();
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

        private async Task LoadParcels()
        {
            if (!_kurierId.HasValue)
            {
                MessageBox.Show("Brak ID kuriera.");
                return;
            }

            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"api/packs/kurier/{_kurierId}?delivered=false");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Błąd pobierania paczek: {response.StatusCode}");
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();

                ParcelResponse wrapper = JsonSerializer.Deserialize<ParcelResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ActiveParcels.Clear();
                foreach (var pack in wrapper?.Values ?? new List<Parcel>())
                {
                    ActiveParcels.Add(new Parcel
                    {
                        PackId = pack.PackId,
                        OrderNumber = pack.PackId.ToString(),
                        LockerCode = pack.KlientName,
                        Size = pack.Size,
                        Delivered = pack.Delivered,
                        Date = pack.Date,
                        KlientName = pack.KlientName
                    });
                }
            }
        }

        private async void DeliverButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var parcel = button?.Tag as Parcel;

            if (parcel == null) return;

            using (var client = CreateHttpClient())
            {
                var response = await client.PutAsync($"api/packs/{parcel.PackId}/deliver", null);

                if (response.IsSuccessStatusCode)
                {
                    ActiveParcels.Remove(parcel);
                    MessageBox.Show($"Paczka {parcel.OrderNumber} została wydana do paczkomatu {parcel.LockerCode}.",
                                    "Status wydania", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Błąd zmiany statusu: {response.StatusCode}",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginPage login = new LoginPage();
            login.Show();
            this.Close();
        }
    }
}