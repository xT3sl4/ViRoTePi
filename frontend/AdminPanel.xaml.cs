using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace frontend
{
    #region Models

    public class UnassignedPack
    {
        public int PackId { get; set; }
        public string Size { get; set; }
        public string Date { get; set; }
        public string ReceiverName { get; set; }
        public string PaczkomatName { get; set; }
        public bool HasKurier { get; set; }
    }

    public class KurierInfo
    {
        public int KurierId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string State { get; set; }
        public int AssignedPacksCount { get; set; }
    }

    public class AdminStats
    {
        public int TotalPacks { get; set; }
        public int UndeliveredPacks { get; set; }
        public int DeliveredPacks { get; set; }
        public int UnassignedPacks { get; set; }
        public int TotalKuriers { get; set; }
        public int ActiveKuriers { get; set; }
        public int PendingPacks { get; set; }
    }

    public class PendingPack
    {
        public int PendingId { get; set; }
        public string Size { get; set; }
        public string CreatedAt { get; set; }
        public string Status { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string PaczkomatName { get; set; }
    }

    public class ReleasePackResponse
    {
        public string Message { get; set; }
        public int PackId { get; set; }
        public string ReceiverName { get; set; }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int? PhoneNumber { get; set; }
    }

    public class ReferencePreservedList<T>
    {
        [JsonPropertyName("$values")]
        public List<T> Values { get; set; }
    }

    #endregion

    #region Converters

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasKurier = (bool)value;
            return hasKurier
                ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
                : new SolidColorBrush(Color.FromRgb(255, 152, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasKurier = (bool)value;
            return hasKurier ? "Przypisana" : "Czeka";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    public partial class AdminPanel : Window
    {
        private ObservableCollection<UnassignedPack> _packs;
        private ObservableCollection<KurierInfo> _kuriers;
        private ObservableCollection<PendingPack> _pendingPacks;
        private ObservableCollection<AdminUserDto> _allUsers;
        private ObservableCollection<AdminUserDto> _filteredUsers;
        private PendingPack _selectedPending;
        private string _selfie;

        public AdminPanel(string selfie = null)
        {
            InitializeComponent();
            _selfie = selfie;
            _packs = new ObservableCollection<UnassignedPack>();
            _kuriers = new ObservableCollection<KurierInfo>();
            _pendingPacks = new ObservableCollection<PendingPack>();
            _allUsers = new ObservableCollection<AdminUserDto>();
            _filteredUsers = new ObservableCollection<AdminUserDto>();

            PacksDataGrid.ItemsSource = _packs;
            KuriersListBox.ItemsSource = _kuriers;
            PendingPacksDataGrid.ItemsSource = _pendingPacks;
            UsersDataGrid.ItemsSource = _filteredUsers;

            PendingPacksDataGrid.SelectionChanged += PendingPacksDataGrid_SelectionChanged;

            LoadProfileImage(_selfie);
            Loaded += async (s, e) => await LoadData();
        }

        private void LoadProfileImage(string selfieFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selfieFileName)) return;
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", selfieFileName);
                if (!System.IO.File.Exists(path)) return;
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ProfileImageBrush.ImageSource = bitmap;
            }
            catch { }
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7272/")
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task LoadData()
        {
            await LoadStats();
            await LoadPacks();
            await LoadKuriers();
            await LoadPendingPacks();
            await LoadUsers();
        }

        private async Task LoadStats()
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.GetAsync("api/admin/stats");
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var stats = JsonSerializer.Deserialize<AdminStats>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (stats != null)
                        {
                            UnassignedCountText.Text = stats.UnassignedPacks.ToString();
                            InTransitCountText.Text = (stats.UndeliveredPacks - stats.UnassignedPacks).ToString();
                            DeliveredCountText.Text = stats.DeliveredPacks.ToString();
                            ActiveKuriersText.Text = $"{stats.ActiveKuriers}/{stats.TotalKuriers}";
                            PendingCountText.Text = stats.PendingPacks.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd pobierania statystyk: {ex.Message}");
                }
            }
        }

        private async Task LoadPacks()
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.GetAsync("api/admin/packs");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var wrapper = JsonSerializer.Deserialize<ReferencePreservedList<UnassignedPack>>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var packs = wrapper?.Values;

                        _packs.Clear();
                        if (packs != null)
                            foreach (var pack in packs)
                                _packs.Add(pack);
                    }
                    else
                    {
                        MessageBox.Show($"Błąd pobierania paczek: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd pobierania paczek: {ex.Message}");
                }
            }
        }

        private async Task LoadKuriers()
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.GetAsync("api/admin/kuriers");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var wrapper = JsonSerializer.Deserialize<ReferencePreservedList<KurierInfo>>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var kuriers = wrapper?.Values;

                        _kuriers.Clear();
                        if (kuriers != null)
                            foreach (var kurier in kuriers)
                                _kuriers.Add(kurier);
                    }
                    else
                    {
                        MessageBox.Show($"Błąd pobierania kurierów: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd pobierania kurierów: {ex.Message}");
                }
            }
        }

        private async Task LoadPendingPacks()
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.GetAsync("api/admin/pending-packs");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var wrapper = JsonSerializer.Deserialize<ReferencePreservedList<PendingPack>>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var pending = wrapper?.Values;

                        _pendingPacks.Clear();
                        if (pending != null)
                            foreach (var p in pending)
                                _pendingPacks.Add(p);
                    }
                    else
                    {
                        MessageBox.Show($"Błąd pobierania oczekujących paczek: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd pobierania oczekujących paczek: {ex.Message}");
                }
            }
        }

        private async Task LoadUsers()
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.GetAsync("api/users");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        List<AdminUserDto> users = null;
                        try
                        {
                            users = JsonSerializer.Deserialize<List<AdminUserDto>>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch
                        {
                            var wrapper = JsonSerializer.Deserialize<ReferencePreservedList<AdminUserDto>>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            users = wrapper?.Values;
                        }

                        _allUsers.Clear();
                        _filteredUsers.Clear();
                        if (users != null)
                            foreach (var u in users)
                            {
                                _allUsers.Add(u);
                                _filteredUsers.Add(u);
                            }

                        UserCountText.Text = $"Użytkowników: {_filteredUsers.Count}";
                    }
                    else
                    {
                        MessageBox.Show($"Błąd pobierania użytkowników: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd pobierania użytkowników: {ex.Message}");
                }
            }
        }

        private void PendingPacksDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedPending = PendingPacksDataGrid.SelectedItem as PendingPack;

            if (_selectedPending != null)
            {
                SelectedPackInfo.Text = $"Paczka #{_selectedPending.PendingId} ({_selectedPending.Size})\n" +
                                       $"Nadawca: {_selectedPending.SenderName}\n" +
                                       $"Odbiorca: {_selectedPending.ReceiverName}\n" +
                                       $"Paczkomat: {_selectedPending.PaczkomatName}";
            }
            else
            {
                SelectedPackInfo.Text = "Nie wybrano";
            }
        }

        private async void AssignBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPending == null)
            {
                MessageBox.Show("Wybierz paczkę oczekującą z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedKurier = KuriersListBox.SelectedItem as KurierInfo;
            if (selectedKurier == null)
            {
                MessageBox.Show("Wybierz kuriera z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var client = CreateHttpClient())
            {
                try
                {
                    var releaseResponse = await client.PostAsync(
                        $"api/admin/release-pack/{_selectedPending.PendingId}", null);

                    if (!releaseResponse.IsSuccessStatusCode)
                    {
                        string error = await releaseResponse.Content.ReadAsStringAsync();
                        MessageBox.Show($"Błąd wydawania paczki: {error}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string releaseJson = await releaseResponse.Content.ReadAsStringAsync();
                    var releaseResult = JsonSerializer.Deserialize<ReleasePackResponse>(releaseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (releaseResult == null || releaseResult.PackId == 0)
                    {
                        MessageBox.Show("Błąd pobierania ID wydanej paczki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var assignRequest = new
                    {
                        PackId = releaseResult.PackId,
                        KurierId = selectedKurier.KurierId
                    };

                    string assignJson = JsonSerializer.Serialize(assignRequest);
                    var assignContent = new StringContent(assignJson, Encoding.UTF8, "application/json");
                    var assignResponse = await client.PostAsync("api/admin/assign-pack", assignContent);

                    if (assignResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"Paczka wydana i przypisana do kuriera {selectedKurier.Name}!",
                            "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        await LoadData();
                        PendingPacksDataGrid.SelectedItem = null;
                        KuriersListBox.SelectedItem = null;
                        SelectedPackInfo.Text = "Nie wybrano";
                        _selectedPending = null;
                    }
                    else
                    {
                        string error = await assignResponse.Content.ReadAsStringAsync();
                        MessageBox.Show($"Paczka wydana ale błąd przypisania kuriera: {error}",
                            "Ostrzeżenie", MessageBoxButton.OK, MessageBoxImage.Warning);
                        await LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ReleasePackBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedPending = PendingPacksDataGrid.SelectedItem as PendingPack;
            if (selectedPending == null)
            {
                MessageBox.Show("Wybierz paczkę oczekującą z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Czy na pewno chcesz wydać paczkę #{selectedPending.PendingId} bez kuriera?\n" +
                $"Odbiorca: {selectedPending.ReceiverName}\n" +
                $"Paczkomat: {selectedPending.PaczkomatName}",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.PostAsync(
                        $"api/admin/release-pack/{selectedPending.PendingId}", null);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"Paczka została wydana dla {selectedPending.ReceiverName}.",
                            "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        await LoadData();
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Błąd wydawania paczki: {errorBody}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RejectPackBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedPending = PendingPacksDataGrid.SelectedItem as PendingPack;
            if (selectedPending == null)
            {
                MessageBox.Show("Wybierz paczkę oczekującą z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Czy na pewno chcesz odrzucić paczkę #{selectedPending.PendingId}?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.DeleteAsync(
                        $"api/admin/reject-pack/{selectedPending.PendingId}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Paczka została odrzucona.",
                            "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);

                        await LoadData();
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Błąd odrzucania: {errorBody}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PacksDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
            MessageBox.Show("Dane zostały odświeżone.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void RefreshUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private void RoleFilterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // filtrowanie usunięte
        }

        private async void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                await LoadUsers();
            }
        }

        private async void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var user = button?.Tag as AdminUserDto;
            if (user == null) return;

            var dialog = new UserEditDialog(user) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                await LoadUsers();
            }
        }

        private async void DeleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var user = button?.Tag as AdminUserDto;
            if (user == null) return;

            var confirm = MessageBox.Show(
                $"Czy na pewno chcesz usunąć użytkownika?\n\n" +
                $"Imię i nazwisko: {user.Name} {user.Surname}\n" +
                $"Email: {user.Email}\n" +
                $"Rola: {user.Role}\n\n" +
                $"Ta operacja jest nieodwracalna!",
                "Potwierdzenie usunięcia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            using (var client = CreateHttpClient())
            {
                try
                {
                    var response = await client.DeleteAsync($"api/users/{user.Id}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"Użytkownik {user.Name} {user.Surname} został usunięty.",
                            "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsers();
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        try
                        {
                            var error = JsonDocument.Parse(errorBody);
                            string msg = error.RootElement.GetProperty("message").GetString() ?? errorBody;
                            MessageBox.Show(msg, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch
                        {
                            MessageBox.Show($"Błąd usuwania: {errorBody}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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