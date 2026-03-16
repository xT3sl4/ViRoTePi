using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace frontend
{
    public class Parcel
    {
        public string OrderNumber { get; set; }
        public string LockerCode { get; set; }
    }

    public partial class CourierPanel : Window
    {
       public ObservableCollection<Parcel> ActiveParcels { get; set; }

        public CourierPanel()
        {
            InitializeComponent();
            ActiveParcels = new ObservableCollection<Parcel>
            {
                new Parcel { OrderNumber = "1002938475", LockerCode = "GD001" },
                new Parcel { OrderNumber = "1002938489", LockerCode = "GD002" },
                new Parcel { OrderNumber = "1002938512", LockerCode = "GD005" },
                new Parcel { OrderNumber = "2001122334", LockerCode = "GD001" }
            };

            // Podpinamy listę do elementu w XAML
            PackagesList.ItemsSource = ActiveParcels;
        }

        private void DeliverButton_Click(object sender, RoutedEventArgs e)
        {

            var button = sender as Button;
            var parcel = button.Tag as Parcel;

            if (parcel != null)
            {
                ActiveParcels.Remove(parcel);

                MessageBox.Show($"Paczka {parcel.OrderNumber} została wydana do paczkomatu {parcel.LockerCode}.",
                                "Status wydania", MessageBoxButton.OK, MessageBoxImage.Information);
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