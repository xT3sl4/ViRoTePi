using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

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
        public string PaczkomatName { get; set; } = "";
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
        private bool _isAvailable = true;

        public CourierPanel(int? kurierId = null, string selfie = null)
        {
            InitializeComponent();
            _kurierId = kurierId;
            ActiveParcels = new ObservableCollection<Parcel>();
            PackagesList.ItemsSource = ActiveParcels;

            LoadProfileImage(selfie);
            Loaded += async (s, e) =>
            {
                await LoadParcels();
                await LoadKurierState();
            };
        }

        private void LoadProfileImage(string selfieFileName)
        {
            try
            {
                string imageFile = string.IsNullOrWhiteSpace(selfieFileName) ? "user.png" : selfieFileName;
                var bitmap = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri($"pack://application:,,,/Images/{imageFile}", UriKind.Absolute));
                ProfileImageBrush.ImageSource = bitmap;
            }
            catch
            {
                try
                {
                    var bmp = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri("pack://application:,,,/Images/user.png", UriKind.Absolute));
                    ProfileImageBrush.ImageSource = bmp;
                }
                catch { }
            }
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
                        KlientName = pack.KlientName,
                        PaczkomatName = pack.PaczkomatName ?? ""
                    });
                }
            }
        }

        private async void RefreshParcels_Click(object sender, RoutedEventArgs e)
        {
            await LoadParcels();
        }

        private void ListViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ListViewPanel.Visibility = Visibility.Visible;
            MapViewPanel.Visibility = Visibility.Collapsed;

            ListViewBtn.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6200"));
            ListViewBtn.Foreground = System.Windows.Media.Brushes.White;
            MapViewBtn.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCD00"));
            MapViewBtn.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333333"));
        }

        private async void MapViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ListViewPanel.Visibility = Visibility.Collapsed;
            MapViewPanel.Visibility = Visibility.Visible;

            MapViewBtn.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6200"));
            MapViewBtn.Foreground = System.Windows.Media.Brushes.White;
            ListViewBtn.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCD00"));
            ListViewBtn.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333333"));

            await LoadCourierMap();
        }

        private WebView2 _courierMapWebView;
        private bool _mapLoaded = false;

        private async Task LoadCourierMap()
        {
            if (_courierMapWebView == null)
            {
                _courierMapWebView = new WebView2();
                MapContainer.Child = _courierMapWebView;

                string tempPath = Path.Combine(
                    Path.GetTempPath(),
                    "InPost_CourierMap_" + Guid.NewGuid().ToString("N").Substring(0, 8));

                var env = await CoreWebView2Environment.CreateAsync(null, tempPath);
                await _courierMapWebView.EnsureCoreWebView2Async(env);

                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string htmlPath = Path.Combine(appDir, "Assets", "map.html");
                if (!File.Exists(htmlPath))
                    htmlPath = Path.Combine(appDir, "map.html");
                if (!File.Exists(htmlPath))
                    htmlPath = Path.GetFullPath(Path.Combine(appDir, "..", "..", "Assets", "map.html"));
                if (!File.Exists(htmlPath))
                    htmlPath = Path.GetFullPath(Path.Combine(appDir, "..", "..", "map.html"));

                if (File.Exists(htmlPath))
                {
                    _courierMapWebView.CoreWebView2.NavigationCompleted += async (s, args) =>
                    {
                        _mapLoaded = true;
                        await InjectParcelMarkers();
                    };
                    _courierMapWebView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
                }
                else
                {
                    MessageBox.Show("Nie znaleziono pliku map.html.", "Błąd mapy",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else if (_mapLoaded)
            {
                await InjectParcelMarkers();
            }
        }


        private async Task InjectParcelMarkers()
        {

            var grouped = ActiveParcels
                .GroupBy(p => p.PaczkomatName ?? "")
                .ToDictionary(g => g.Key, g => g.ToList());

            string js = @"
                // Usuń oryginalne markery z map.html (przy pierwszym załadowaniu)
                if (!window._originalMarkersRemoved) {
                    map.eachLayer(function(layer) {
                        if (layer instanceof L.Marker) {
                            map.removeLayer(layer);
                        }
                    });
                    window._originalMarkersRemoved = true;
                }

                // Usuń stare markery kuriera jeśli istnieją
                if (window._courierMarkers) {
                    window._courierMarkers.forEach(m => map.removeLayer(m));
                }
                window._courierMarkers = [];

                // Dane paczek pogrupowane po paczkomacie
                var packsByLocker = " + JsonSerializer.Serialize(
                    grouped.ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.Select(p => new { id = p.PackId, size = p.Size }).ToList()
                    )) + @";

                // Dla każdego paczkomatu z oryginalnej listy (z map.html)
                paczkomaty.forEach(function(p) {
                    var packs = packsByLocker[p.name] || [];
                    var count = packs.length;

                    // Tworzymy nowy marker z ikoną z liczbą
                    var icon;
                    if (count > 0) {
                        icon = L.divIcon({
                            className: 'courier-marker',
                            html: '<div style=""background:#FF6200;color:white;border-radius:50%;width:32px;height:32px;display:flex;align-items:center;justify-content:center;font-weight:bold;font-size:14px;border:3px solid white;box-shadow:0 2px 6px rgba(0,0,0,0.3);"">' + count + '</div>',
                            iconSize: [32, 32],
                            iconAnchor: [16, 16]
                        });
                    } else {
                        icon = L.divIcon({
                            className: 'courier-marker',
                            html: '<div style=""background:#9E9E9E;color:white;border-radius:50%;width:24px;height:24px;display:flex;align-items:center;justify-content:center;font-weight:bold;font-size:11px;border:2px solid white;box-shadow:0 2px 4px rgba(0,0,0,0.2);opacity:0.6;"">0</div>',
                            iconSize: [24, 24],
                            iconAnchor: [12, 12]
                        });
                    }

                    var marker = L.marker([p.lat, p.lon], { icon: icon }).addTo(map);

                    // Popup z listą paczek
                    var popupHtml = '<b>Paczkomat: ' + p.name + '</b><br>';
                    if (count === 0) {
                        popupHtml += '<span style=""color:gray;"">Brak paczek do dostarczenia</span>';
                    } else {
                        popupHtml += '<b>' + count + ' paczek do dostarczenia:</b><br>';
                        packs.forEach(function(pack) {
                            popupHtml += '• Paczka #' + pack.id + ' (rozmiar: ' + pack.size + ')<br>';
                        });
                    }
                    marker.bindPopup(popupHtml);

                    window._courierMarkers.push(marker);
                });
            ";

            try
            {
                await _courierMapWebView.CoreWebView2.ExecuteScriptAsync(js);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd ładowania markerów na mapie: " + ex.Message);
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

        private async Task LoadKurierState()
        {
            if (!_kurierId.HasValue) return;

            try
            {
                using (var client = CreateHttpClient())
                {
                    var response = await client.GetAsync("api/admin/kuriers");
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                        List<KurierInfo> kuriers;
                        try
                        {
                            var wrapper = JsonSerializer.Deserialize<KuriersWrapper>(json, options);
                            kuriers = wrapper?.Values ?? new List<KurierInfo>();
                        }
                        catch
                        {
                            kuriers = JsonSerializer.Deserialize<List<KurierInfo>>(json, options) ?? new List<KurierInfo>();
                        }

                        var myKurier = kuriers.FirstOrDefault(k => k.KurierId == _kurierId.Value);
                        if (myKurier != null)
                        {
                            _isAvailable = myKurier.State == "available";
                            UpdateStatusUI();
                        }
                    }
                }
            }
            catch { }
        }

        private void UpdateStatusUI()
        {
            if (_isAvailable)
            {
                StatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4CAF50"));
                StatusText.Text = "Dostępny";
            }
            else
            {
                StatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9E9E9E"));
                StatusText.Text = "Niedostępny";
            }
        }

        private async void StatusToggle_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_kurierId.HasValue) return;

            try
            {
                using (var client = CreateHttpClient())
                {
                    var response = await client.PutAsync($"api/admin/kurier/{_kurierId}/toggle-state", null);
                    if (response.IsSuccessStatusCode)
                    {
                        _isAvailable = !_isAvailable;
                        UpdateStatusUI();
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się zmienić statusu.", "Błąd",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia: " + ex.Message, "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginPage login = new LoginPage();
            login.Show();
            this.Close();
        }
    }

    public class KuriersWrapper
    {
        [JsonPropertyName("$values")]
        public List<KurierInfo> Values { get; set; } = new List<KurierInfo>();
    }
}