using System;
using System.Windows;

namespace frontend
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Packages_Click(object sender, RoutedEventArgs e)
        {
            PackagesPanel.Visibility = Visibility.Visible;
            SendPackagePanel.Visibility = Visibility.Collapsed;
        }

        public void SendPackage_Click(object sender, RoutedEventArgs e)
        {
            PackagesPanel.Visibility = Visibility.Collapsed;
            SendPackagePanel.Visibility = Visibility.Visible;
        }

        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            OtworzMape();
        }

        private void LockerButton_Click(object sender, RoutedEventArgs e)
        {
            OtworzMape();
        }

        private void OtworzMape()
        {
            // Tworzymy nowe okno
            CustomMessageBox mapaWindow = new CustomMessageBox();
            mapaWindow.Owner = this;

            // Otwieramy i sprawdzamy czy użytkownik coś wybrał (DialogResult = true)
            if (mapaWindow.ShowDialog() == true)
            {
                var wybrany = mapaWindow.WybranyPaczkomat;
                if (wybrany != null)
                {
                    // Wpisujemy nazwę do UI w MainWindow
                    SelectedLockerInfo.Text = $"Wybrany punkt: {wybrany.name}";
                    SelectedLockerInfo.FontStyle = FontStyles.Normal;
                    SelectedLockerInfo.FontWeight = FontWeights.Bold;

                    // Automatycznie przełączamy na panel wysyłki, jeśli użytkownik był na mapie głównej
                    SendPackage_Click(null, null);
                }
            }
        }

        public void User_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Profil użytkownika");

        public class PaczkomatMapaData
        {
            public string name { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }
    }
}