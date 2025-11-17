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
using MySql.Data.MySqlClient;

namespace Kurierstwo
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 1. Skonfiguruj Connection String
            // Uzupełnij "database=" nazwą swojej bazy z phpMyAdmin
            string connectionString = "server=localhost;database=inpost;user=root;password=";

            try
            {
                // 2. Utwórz połączenie
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // 3. Otwórz połączenie
                    conn.Open();

                    // Jeśli kod dotarł tutaj, to znaczy, że się udało!
                    MessageBox.Show("Połączono z bazą danych pomyślnie!", "Sukces");

                    // Tutaj możesz wykonywać zapytania SQL...

                    // Połączenie zamknie się automatycznie po wyjściu z bloku 'using'
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędów - np. złe hasło lub wyłączony serwer
                MessageBox.Show($"Błąd połączenia:\n{ex.Message}", "Błąd");
            }
        }
    }
}
