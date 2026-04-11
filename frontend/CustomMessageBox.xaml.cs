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
                string tempPath = Path.Combine(Path.GetTempPath(), "InPost_Map_Session_" + Guid.NewGuid().ToString().Substring(0, 5));
                var env = await CoreWebView2Environment.CreateAsync(null, tempPath);
                await MapWebView.EnsureCoreWebView2Async(env);

                MapWebView.CoreWebView2.WebMessageReceived += OnMapMessageReceived;

                string htmlPath = @"C:\Users\wjane\Desktop\szkola\projekt\map.html";
                if (File.Exists(htmlPath))
                    MapWebView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
            }
            catch (Exception ex) { MessageBox.Show("Błąd mapy: " + ex.Message); }
        }

        private void OnMapMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var p = JsonConvert.DeserializeObject<MainWindow.PaczkomatMapaData>(e.WebMessageAsJson);
            if (p != null)
            {
                WybranyPaczkomat = p;
                SelectedInfo.Text = "Wybrano: " + p.name;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (WybranyPaczkomat != null) { this.DialogResult = true; this.Close(); }
            else { MessageBox.Show("Wybierz punkt na mapie!"); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}