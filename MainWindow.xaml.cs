using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace frontend
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void User_Click(object sender, RoutedEventArgs e)
        {
            LoginPage userWindow = new LoginPage();
            userWindow.Show();
            this.Close();
        }
        public class Paczka
        {
            public string NumerPaczki { get; set; }
            public string Status { get; set; }
            public string NazwaPaczkomatu { get; set; }
        }


    }
}
