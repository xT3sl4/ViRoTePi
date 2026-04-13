using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using frontend;

namespace frontend
{

    public class DeliveredToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string) == "Doręczona" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        private int? klientId;
        private int? _userId;
        private string _selectedPaczkomatName = null;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureUIForGuest();
        }

        public MainWindow(int? klientId, string selfie = null, int? userId = null)
        {
            InitializeComponent();
            this.klientId = klientId;
            this._userId = userId;

            if (klientId.HasValue)
            {

                ConfigureUIForLoggedIn(selfie, userId);

                Loaded += async (s, e) =>
                {
                    List<Pack> packs = await GetPacksAsync(this.klientId);
                    PacksDataGrid.ItemsSource = packs;
                };
            }
            else
            {
                ConfigureUIForGuest();
            }
        }



        private void ConfigureUIForLoggedIn(string selfieFileName, int? userId)
        {

            LoggedInPanel.Visibility = Visibility.Visible;
            LoginBtn.Visibility = Visibility.Collapsed;


            LoadSelfie(selfieFileName);


            if (userId.HasValue)
                _ = LoadUserNameAsync(userId.Value);


            HideAllPanels();
            PackagesPanel.Visibility = Visibility.Visible;
        }

        private void ConfigureUIForGuest()
        {

            LoggedInPanel.Visibility = Visibility.Collapsed;
            LoginBtn.Visibility = Visibility.Visible;
            LoadSelfie(null); 

            HideAllPanels();
            SendPackagePanel.Visibility = Visibility.Visible;
        }

        private void LoadSelfie(string selfieFileName)
        {
            try
            {
                string imageFile = selfieFileName;


                if (string.IsNullOrWhiteSpace(imageFile))
                    imageFile = "user.png";

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

        private async Task LoadUserNameAsync(int userId)
        {
            try
            {
                using (var client = CreateHttpClient())
                {
                    var response = await client.GetAsync($"api/users/{userId}");
                    if (!response.IsSuccessStatusCode) return;

                    string json = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserProfile>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (user != null)
                        UserNameText.Text = $"{user.Name} {user.Surname}";
                }
            }
            catch { }
        }


        public class Pack
        {
            public int PackId { get; set; }
            public string Size { get; set; }
            public string Delivered { get; set; } = "";
            public string Date { get; set; }
            public string KlientName { get; set; } = "";
            public bool PickedUp { get; set; } = false;
        }

        public class ClientInfo
        {
            public int KlientId { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public int? Phone { get; set; }
            public string Selfie { get; set; }
        }

        public class ClientsResponse
        {
            [JsonPropertyName("$values")]
            public List<ClientInfo> Values { get; set; } = new List<ClientInfo>();
        }

        public class PackResponse
        {
            [JsonPropertyName("$values")]
            public List<Pack> Values { get; set; } = new List<Pack>();
        }

        public class PaczkomatMapaData
        {
            public int id { get; set; }
            public string name { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class CheckResponse
        {
            public bool Exists { get; set; }
        }

        public class UserProfile
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public int? PhoneNumber { get; set; }
            public string Role { get; set; }
        }


        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7272/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<List<Pack>> GetPacksAsync(int? klientId = null)
        {
            using (HttpClient client = CreateHttpClient())
            {
                string url = $"api/packs?klientId={klientId}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Błąd pobierania paczek: {response.StatusCode}");
                    return new List<Pack>();
                }

                string json = await response.Content.ReadAsStringAsync();
                PackResponse wrapper = JsonSerializer.Deserialize<PackResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapper?.Values ?? new List<Pack>();
            }
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            using (HttpClient client = CreateHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(
                        $"api/auth/check-email?email={Uri.EscapeDataString(email)}");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<CheckResponse>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return result?.Exists ?? false;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        private async Task<bool> CheckPhoneExistsAsync(string phone)
        {
            using (HttpClient client = CreateHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(
                        $"api/auth/check-phone?phone={Uri.EscapeDataString(phone)}");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<CheckResponse>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return result?.Exists ?? false;
                    }
                    return false;
                }
                catch { return false; }
            }
        }


        private void HideAllPanels()
        {
            PackagesPanel.Visibility = Visibility.Collapsed;
            SendPackagePanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility = Visibility.Collapsed;
            SendToClientPanel.Visibility = Visibility.Collapsed;
        }

        public async void Packages_Click(object sender, RoutedEventArgs e)
        {
            if (!klientId.HasValue)
            {
                MessageBox.Show("Musisz być zalogowany, aby zobaczyć swoje paczki.",
                    "Wymagane logowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            HideAllPanels();
            PackagesPanel.Visibility = Visibility.Visible;

            List<Pack> packs = await GetPacksAsync(klientId);
            PacksDataGrid.ItemsSource = packs;
        }

        public void SendPackage_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            SendPackagePanel.Visibility = Visibility.Visible;
        }

        public async void History_Click(object sender, RoutedEventArgs e)
        {
            if (!klientId.HasValue)
            {
                MessageBox.Show("Musisz być zalogowany, aby zobaczyć historię paczek.",
                    "Wymagane logowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            HideAllPanels();
            HistoryPanel.Visibility = Visibility.Visible;

            List<Pack> history = await GetPackHistoryAsync();
            HistoryDataGrid.ItemsSource = history;
        }

        public async void SendToClient_Click(object sender, RoutedEventArgs e)
        {
            if (!klientId.HasValue)
            {
                MessageBox.Show("Musisz być zalogowany, aby wysyłać paczki do klientów.",
                    "Wymagane logowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            HideAllPanels();
            SendToClientPanel.Visibility = Visibility.Visible;

            await LoadClientsListAsync();
        }

        private void MapButton_Click(object sender, RoutedEventArgs e) => OtworzMape();
        private void LockerButton_Click(object sender, RoutedEventArgs e) => OtworzMape();


        public void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.Owner = this;
            helpWindow.ShowDialog();
        }


        private async void PickupPack_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var pack = button?.Tag as Pack;
            if (pack == null) return;

            var confirm = MessageBox.Show(
                $"Czy na pewno chcesz odebrać paczkę nr {pack.PackId}?",
                "Potwierdzenie odbioru",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            using (var client = CreateHttpClient())
            {
                try
                {
                    string url = $"api/packs/{pack.PackId}/pickup";
                    if (klientId.HasValue)
                        url += $"?klientId={klientId.Value}";

                    var response = await client.PutAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"Paczka nr {pack.PackId} została odebrana!",
                            "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        List<Pack> packs = await GetPacksAsync(klientId);
                        PacksDataGrid.ItemsSource = packs;
                    }
                    else
                    {
                        string body = await response.Content.ReadAsStringAsync();
                        try
                        {
                            var error = JsonDocument.Parse(body);
                            string msg = error.RootElement.GetProperty("message").GetString() ?? body;
                            MessageBox.Show(msg, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch
                        {
                            MessageBox.Show($"Błąd: {body}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd połączenia: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public void User_Click(object sender, RoutedEventArgs e)
        {
            if (_userId.HasValue)
            {
                OpenProfileEdit();
            }
            else
            {

                LoginPage loginWindow = new LoginPage();
                loginWindow.Show();
                this.Close();
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Czy na pewno chcesz się wylogować?",
                "Wylogowanie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            LoginPage loginPage = new LoginPage();
            loginPage.Show();
            this.Close();
        }

        private async void OpenProfileEdit()
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"api/users/{_userId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Nie udało się pobrać danych profilu.", "Błąd",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserProfile>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (user == null) return;

                    var editWindow = new ProfileEditWindow(
                        userId: _userId.Value,
                        name: user.Name ?? "",
                        surname: user.Surname ?? "",
                        email: user.Email ?? "",
                        phone: user.PhoneNumber?.ToString() ?? ""
                    );
                    editWindow.Owner = this;
                    editWindow.ProfileUpdated += () =>
                    {

                        _ = LoadUserNameAsync(_userId.Value);
                    };
                    editWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private async Task<List<Pack>> GetPackHistoryAsync()
        {
            using (HttpClient client = CreateHttpClient())
            {
                string url = _userId.HasValue
                    ? $"api/packs/history?userId={_userId.Value}"
                    : $"api/packs/history?klientId={klientId}";

                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Błąd pobierania historii: {response.StatusCode}");
                    return new List<Pack>();
                }

                string json = await response.Content.ReadAsStringAsync();
                PackResponse wrapper = JsonSerializer.Deserialize<PackResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapper?.Values ?? new List<Pack>();
            }
        }



        private List<ClientInfo> _clientsList = new List<ClientInfo>();
        private ClientInfo _selectedClient = null;
        private string _clientSelectedPaczkomatName = null;
        private string _clientSelectedSize = null;

        private async Task LoadClientsListAsync()
        {
            using (HttpClient client = CreateHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("api/packs/clients-list");
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Błąd pobierania listy klientów: {response.StatusCode}");
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var wrapper = JsonSerializer.Deserialize<ClientsResponse>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        _clientsList = wrapper?.Values ?? new List<ClientInfo>();
                    }
                    catch
                    {
                        _clientsList = JsonSerializer.Deserialize<List<ClientInfo>>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ClientInfo>();
                    }


                    if (klientId.HasValue)
                        _clientsList.RemoveAll(c => c.KlientId == klientId.Value);

                    ClientsListBox.Items.Clear();
                    foreach (var cl in _clientsList)
                    {
                        var sp = new System.Windows.Controls.StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Horizontal
                        };

                        var ellipse = new System.Windows.Shapes.Ellipse
                        {
                            Width = 48,
                            Height = 48,
                            Margin = new Thickness(0, 0, 12, 0)
                        };

                        try
                        {
                            string imgFile = string.IsNullOrWhiteSpace(cl.Selfie) ? "user.png" : cl.Selfie;
                            var bitmap = new System.Windows.Media.Imaging.BitmapImage(
                                new Uri($"pack://application:,,,/Images/{imgFile}", UriKind.Absolute));
                            ellipse.Fill = new System.Windows.Media.ImageBrush(bitmap)
                            {
                                Stretch = System.Windows.Media.Stretch.UniformToFill
                            };
                        }
                        catch
                        {
                            var bmp = new System.Windows.Media.Imaging.BitmapImage(
                                new Uri("pack://application:,,,/Images/user.png", UriKind.Absolute));
                            ellipse.Fill = new System.Windows.Media.ImageBrush(bmp)
                            {
                                Stretch = System.Windows.Media.Stretch.UniformToFill
                            };
                        }

                        sp.Children.Add(ellipse);

                        var textPanel = new System.Windows.Controls.StackPanel();
                        textPanel.Children.Add(new System.Windows.Controls.TextBlock
                        {
                            Text = $"{cl.Name} {cl.Surname}",
                            FontSize = 16,
                            FontWeight = FontWeights.Bold
                        });
                        textPanel.Children.Add(new System.Windows.Controls.TextBlock
                        {
                            Text = cl.Email ?? "",
                            FontSize = 12,
                            Foreground = System.Windows.Media.Brushes.Gray
                        });
                        if (cl.Phone.HasValue)
                        {
                            textPanel.Children.Add(new System.Windows.Controls.TextBlock
                            {
                                Text = cl.Phone.Value.ToString("000 000 000"),
                                FontSize = 12,
                                Foreground = System.Windows.Media.Brushes.Gray
                            });
                        }
                        sp.Children.Add(textPanel);

                        var item = new System.Windows.Controls.ListBoxItem
                        {
                            Content = sp,
                            Tag = cl
                        };
                        ClientsListBox.Items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClientsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = ClientsListBox.SelectedItem as System.Windows.Controls.ListBoxItem;
            if (selectedItem?.Tag is ClientInfo ci)
            {
                _selectedClient = ci;
                SelectedClientInfo.Text = $"Odbiorca: {ci.Name} {ci.Surname}";
                SelectedClientInfo.FontStyle = FontStyles.Normal;
                SelectedClientInfo.FontWeight = FontWeights.Bold;
            }
        }

        private void client_small_parcel_Selected(object sender, RoutedEventArgs e)
        {
            _clientSelectedSize = "S";
            ClientPriceText.Text = "Do zapłaty: 14.99 zł";
        }

        private void client_medium_parcel_Selected(object sender, RoutedEventArgs e)
        {
            _clientSelectedSize = "M";
            ClientPriceText.Text = "Do zapłaty: 16.99 zł";
        }

        private void client_big_parcel_Selected(object sender, RoutedEventArgs e)
        {
            _clientSelectedSize = "L";
            ClientPriceText.Text = "Do zapłaty: 19.99 zł";
        }

        private void ClientLockerButton_Click(object sender, RoutedEventArgs e)
        {
            MapPickerWindow mapaWindow = new MapPickerWindow { Owner = this };
            if (mapaWindow.ShowDialog() == true)
            {
                var wybrany = mapaWindow.WybranyPaczkomat;
                if (wybrany != null)
                {
                    _clientSelectedPaczkomatName = wybrany.name;
                    ClientSelectedLockerInfo.Text = $"Wybrany punkt: {wybrany.name}";
                    ClientSelectedLockerInfo.FontStyle = FontStyles.Normal;
                    ClientSelectedLockerInfo.FontWeight = FontWeights.Bold;
                }
            }
        }

        private async void SendToClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                MessageBox.Show("Wybierz odbiorcę z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_clientSelectedSize))
            {
                MessageBox.Show("Wybierz rozmiar paczki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_clientSelectedPaczkomatName))
            {
                MessageBox.Show("Wybierz paczkomat z mapy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string senderEmail = "";
            if (_userId.HasValue)
            {
                using (var client = CreateHttpClient())
                {
                    var resp = await client.GetAsync($"api/users/{_userId.Value}");
                    if (resp.IsSuccessStatusCode)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        var user = JsonSerializer.Deserialize<UserProfile>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        senderEmail = user?.Email ?? "";
                    }
                }
            }

            var request = new SendPackRequest
            {
                ReceiverEmail = senderEmail,
                ReceiverPhone = _selectedClient.Phone?.ToString() ?? "",
                Size = _clientSelectedSize,
                PaczkomatName = _clientSelectedPaczkomatName
            };

            using (var client = CreateHttpClient())
            {
                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/packs/send", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Paczka do {_selectedClient.Name} {_selectedClient.Surname} została wysłana!",
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

 
                    _selectedClient = null;
                    _clientSelectedPaczkomatName = null;
                    _clientSelectedSize = null;
                    ClientsListBox.SelectedIndex = -1;
                    SelectedClientInfo.Text = "Nie wybrano odbiorcy";
                    SelectedClientInfo.FontStyle = FontStyles.Italic;
                    SelectedClientInfo.FontWeight = FontWeights.Normal;
                    ClientSelectedLockerInfo.Text = "Nie wybrano paczkomatu";
                    ClientSelectedLockerInfo.FontStyle = FontStyles.Italic;
                    ClientSelectedLockerInfo.FontWeight = FontWeights.Normal;
                    ClientPriceText.Text = "";
                    ClientParcelSizeListBox.SelectedIndex = -1;
                }
                else
                {
                    try
                    {
                        var error = JsonDocument.Parse(responseBody);
                        string msg = error.RootElement.GetProperty("message").GetString() ?? responseBody;
                        MessageBox.Show(msg, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch
                    {
                        MessageBox.Show($"Błąd: {responseBody}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }



        private void small_parcel_Selected(object sender, RoutedEventArgs e) => price_to_pay.Text = "Do zapłaty: 14.99 zł";
        private void medium_parcel_Selected(object sender, RoutedEventArgs e) => price_to_pay.Text = "Do zapłaty: 16.99 zł";
        private void big_parcel_Selected(object sender, RoutedEventArgs e) => price_to_pay.Text = "Do zapłaty: 19.99 zł";

        private void PhoneSlider_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L) PhoneSlider.Value = Math.Min(PhoneSlider.Maximum, PhoneSlider.Value + 100);
            else if (e.Key == Key.J) PhoneSlider.Value = Math.Max(PhoneSlider.Minimum, PhoneSlider.Value - 100);
        }

        private void PhoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PhoneValueText != null)
                PhoneValueText.Text = ((long)e.NewValue).ToString("000 000 000");
        }



        public class SendPackRequest
        {
            public string ReceiverEmail { get; set; }
            public string ReceiverPhone { get; set; }
            public string Size { get; set; }
            public string PaczkomatName { get; set; }
            public int SenderKlientId { get; set; }
        }

        private int? _selectedPaczkomatId = null;

        private void OtworzMape()
        {
            MapPickerWindow mapaWindow = new MapPickerWindow { Owner = this };
            if (mapaWindow.ShowDialog() == true)
            {
                var wybrany = mapaWindow.WybranyPaczkomat;
                if (wybrany != null)
                {
                    _selectedPaczkomatName = wybrany.name;
                    SelectedLockerInfo.Text = $"Wybrany punkt: {wybrany.name}";
                    SelectedLockerInfo.FontStyle = FontStyles.Normal;
                    SelectedLockerInfo.FontWeight = FontWeights.Bold;
                    SendPackage_Click(null, null);
                }
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string senderEmail = EmailAdres_TextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                MessageBox.Show("Podaj email nadawcy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string receiverPhone = PhoneValueText.Text.Replace(" ", "").Trim();
            if (string.IsNullOrWhiteSpace(receiverPhone))
            {
                MessageBox.Show("Podaj numer telefonu odbiorcy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_selectedPaczkomatName))
            {
                MessageBox.Show("Wybierz paczkomat z mapy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string size = null;
            if (small_parcel.IsSelected) size = "S";
            else if (medium_parcel.IsSelected) size = "M";
            else if (big_parcel.IsSelected) size = "L";

            if (size == null)
            {
                MessageBox.Show("Wybierz rozmiar paczki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool receiverExists = await CheckPhoneExistsAsync(receiverPhone);
            if (!receiverExists)
            {
                MessageBox.Show("Brak konta powiązanego z tym numerem telefonu odbiorcy.\n\n" +
                    "Odbiorca musi posiadać konto w systemie, aby otrzymać paczkę.",
                    "Odbiorca nieznaleziony", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!klientId.HasValue)
            {
                bool senderExists = await CheckEmailExistsAsync(senderEmail);
                if (!senderExists)
                {
                    var result = MessageBox.Show(
                        $"Nie znaleziono konta powiązanego z adresem email: {senderEmail}\n\n" +
                        "Czy chcesz utworzyć nowe konto?",
                        "Brak konta nadawcy",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        RegisterWindow registerWindow = new RegisterWindow(senderEmail);
                        registerWindow.ShowDialog();

                        bool nowExists = await CheckEmailExistsAsync(senderEmail);
                        if (!nowExists)
                        {
                            MessageBox.Show("Rejestracja nie została ukończona. Spróbuj ponownie.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    else return;
                }
            }

            var request = new SendPackRequest
            {
                ReceiverEmail = senderEmail,
                ReceiverPhone = receiverPhone,
                Size = size,
                PaczkomatName = _selectedPaczkomatName
            };

            using (var client = CreateHttpClient())
            {
                string json = System.Text.Json.JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/packs/send", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Paczka została wysłana pomyślnie!", "Sukces",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    EmailAdres_TextBox.Text = "";
                    PhoneValueText.Text = "500 000 000";
                    PhoneSlider.Value = 500000000;
                    SelectedLockerInfo.Text = "Nie wybrano paczkomatu";
                    SelectedLockerInfo.FontStyle = FontStyles.Italic;
                    SelectedLockerInfo.FontWeight = FontWeights.Normal;
                    _selectedPaczkomatName = null;
                }
                else
                {
                    try
                    {
                        var error = System.Text.Json.JsonDocument.Parse(responseBody);
                        string msg = error.RootElement.GetProperty("message").GetString() ?? responseBody;
                        MessageBox.Show(msg, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch
                    {
                        MessageBox.Show($"Błąd: {responseBody}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}