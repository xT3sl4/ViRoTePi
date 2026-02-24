using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Google.Apis.Auth.OAuth2;

namespace frontend
{
    public partial class LoginPage : Window
    {

        private List<User> _database = new List<User>
        {
            new User { Username = "kurier1", Password = "123", Role = "Courier" },
            new User { Username = "admin", Password = "admin", Role = "Admin" },
            new User { Username = "cos", Password = "321", Role = "Klient" }
        };

        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string inputLogin = txtUsername.Text;
            string inputPass = txtPassword.Password;

            var user = _database.FirstOrDefault(u => u.Username == inputLogin && u.Password == inputPass);

            if (user != null)
            {
                if (user.Role == "Courier")
                {
                    CourierPanel panel = new CourierPanel();
                    panel.Show();
                    this.Close();
                }
                else if(user.Role== "Klient")
                {
                    MainWindow panel = new MainWindow();
                    panel.Show();
                    this.Close();
                }

            }
            else
            {
                MessageBox.Show("Błędne dane logowania!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void LoginGoogle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UserCredential credential;

                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { "profile", "email" }, // O co pytamy Google
                        "user",
                        CancellationToken.None
                    );
                }

                if (credential != null && !string.IsNullOrEmpty(credential.Token.AccessToken))
                {
                    CourierPanel panel = new CourierPanel();
                    panel.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas logowania: " + ex.Message);
            }
        }
    }
}