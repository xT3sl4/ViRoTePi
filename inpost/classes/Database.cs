using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace inpost.classes
{
    internal class Database
    {
        string connectionString = "server=localhost;database=inpost;user=root;password=";
        MySqlConnection _mySqlConnection;
        Database _instance = null;
        public Database()
        {
            try
            {
                _mySqlConnection = new MySqlConnection(connectionString);

                MessageBox.Show("Połączono z bazą danych MySQL.", "Sukces");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd połączenia:\n{ex.Message}", "Błąd");
            }
        }
        public IEnumerable<Package> GetPackages()
        {
            IEnumerable<Package> packages = _mySqlConnection.Query<Package>("SELECT * FROM packages;");
            return packages;
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _mySqlConnection.Query<User>("SELECT * FROM users;");
            return users;
        }

    }
}
