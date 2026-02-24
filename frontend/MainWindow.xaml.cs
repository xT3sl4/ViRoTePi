using System;
using System.Data.SqlTypes;
using System.Windows;
using System.Windows.Input;

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
            CustomMessageBox mapaWindow = new CustomMessageBox();
            mapaWindow.Owner = this;

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


        public class PaczkomatMapaData
        {
            public string name { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }

        private void small_parcel_Selected(object sender, RoutedEventArgs e)
        {
            price_to_pay.Text = "Do zapłaty: 14.99 zł";
        }

        private void medium_parcel_Selected(object sender, RoutedEventArgs e)
        {
            price_to_pay.Text = "Do zapłaty: 16.99 zł";
        }

        private void big_parcel_Selected(object sender, RoutedEventArgs e)
        {
            price_to_pay.Text = "Do zapłaty: 19.99 zł";
        }
        
        private void PhoneSlider_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L)
            {
                PhoneSlider.Value = Math.Min(PhoneSlider.Maximum, PhoneSlider.Value + 100);
                e.Handled = true;
            }
            else if (e.Key == Key.J)
            {
                PhoneSlider.Value = Math.Max(PhoneSlider.Minimum, PhoneSlider.Value - 100);
                e.Handled = true;
            }
        }
        private void PhoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PhoneValueText != null)
            {
                PhoneValueText.Text = ((long)e.NewValue).ToString("000 000 000");
            }
        }

        
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Imie: " + Firstname_TextBox.Text + "\n" + "Nazwisko: " + Surname_TextBox.Text + "\n" + "Email: " + EmailAdres_TextBox.Text + "\n" +"Numer Telefonu: " + PhoneValueText.Text + "\n" + SelectedLockerInfo.Text);
        }
    }
}