using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;

namespace frontend
{
    public partial class CustomMessageBox : Window
    {
        public MainWindow.PaczkomatMapaData WybranyPaczkomat { get; private set; }

        public CustomMessageBox()
        {
            InitializeComponent();
            InitializeMap();
        }

        private async void InitializeMap()
        {
            try
            {
                string tempPath = Path.Combine(
                    Path.GetTempPath(),
                    "InPost_Map_" + Guid.NewGuid().ToString("N").Substring(0, 8)
                );

                var env = await CoreWebView2Environment.CreateAsync(null, tempPath);
                await MapWebView.EnsureCoreWebView2Async(env);

                MapWebView.CoreWebView2.WebMessageReceived += OnMapMessageReceived;

                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string htmlPath = Path.Combine(appDir, "map.html");

                if (!File.Exists(htmlPath))
                {
                    htmlPath = Path.GetFullPath(Path.Combine(appDir, "..", "..", "map.html"));
                }
                if (!File.Exists(htmlPath))
                {
                    htmlPath = Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", "map.html"));
                }

                if (File.Exists(htmlPath))
                {
                    MapWebView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
                }
                else
                {
                    MessageBox.Show(
                        "Nie znaleziono pliku map.html.\n\n" +
                        "Skopiuj map.html do folderu z plikiem frontend.exe\n" +
                        "(zazwyczaj bin\\Debug\\)",
                        "Błąd mapy", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd inicjalizacji mapy: " + ex.Message,
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMapMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.TryGetWebMessageAsString();

                if (string.IsNullOrEmpty(json))
                    json = e.WebMessageAsJson; 

                var p = JsonConvert.DeserializeObject<MainWindow.PaczkomatMapaData>(json);

                if (p != null)
                {
                    WybranyPaczkomat = p;
                    Dispatcher.Invoke(() =>
                    {
                        SelectedInfo.Text = "Wybrano: " + p.name;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd odczytu danych z mapy: " + ex.Message);
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (WybranyPaczkomat != null)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Kliknij na marker paczkomatu na mapie, aby go wybrać.",
                    "Brak wyboru", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}