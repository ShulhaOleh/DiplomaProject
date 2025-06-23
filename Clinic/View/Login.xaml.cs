using Clinic.DB;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Clinic.View
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            SetInitialLanguage();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SavedUsername))
                UsernameTextBox.Text = DecodeBase64(Properties.Settings.Default.SavedUsername);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SavedPassword))
                PasswordBox.Password = DecodeBase64(Properties.Settings.Default.SavedPassword);

            RememberMeCheckBox.IsChecked = Properties.Settings.Default.RememberMe;
        }

        private void SetInitialLanguage()
        {
            string culture = Properties.Settings.Default.AppLanguage;
            foreach (ComboBoxItem item in LanguageSelector.Items)
            {
                if (item.Tag.ToString() == culture)
                {
                    LanguageSelector.SelectedItem = item;
                    break;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.LoginViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
                if (vm.RememberMe)
                    Properties.Settings.Default.SavedPassword = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(vm.Password));
            }
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is ComboBoxItem selected)
            {
                string lang = selected.Tag.ToString();
                ChangeLanguage(lang);
                ViewModels.LanguageManager.SetLanguage(lang);
            }
        }

        private void ChangeLanguage(string lang)
        {
            var dict = new ResourceDictionary();
            dict.Source = new Uri($"Languages/Resources.{lang}.xaml", UriKind.Relative);

            var old = Application.Current.Resources.MergedDictionaries
                         .FirstOrDefault(d => d.Source.OriginalString.Contains("Resources."));
            if (old != null)
                Application.Current.Resources.MergedDictionaries.Remove(old);

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            MessageTextBlock.Text = "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageTextBlock.Text = (string)FindResource("EmptyLoginFieldsMessage");
                return;
            }

            try
            {
                using var conn = AuthDB.GetConnection();
                conn.Open();

                string hash = GetMd5Hash(password);
                using var cmd = new MySqlCommand(
                    "SELECT Role, LinkedID FROM Users WHERE Username=@u AND PasswordHash=@p",
                    conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", hash);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    MessageTextBlock.Text = (string)FindResource("IncorrectPasswordOrLoginMessage");
                    return;
                }

                string role = reader.GetString(reader.GetOrdinal("Role"));
                int idOrd = reader.GetOrdinal("LinkedID");
                int? linkedId = reader.IsDBNull(idOrd)
                                ? (int?)null
                                : reader.GetInt32(idOrd);
                reader.Close();

                if (RememberMeCheckBox.IsChecked == true)
                {
                    Properties.Settings.Default.SavedUsername = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
                    Properties.Settings.Default.SavedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
                    Properties.Settings.Default.RememberMe = true;
                }
                else
                {
                    Properties.Settings.Default.SavedUsername = "";
                    Properties.Settings.Default.SavedPassword = "";
                    Properties.Settings.Default.RememberMe = false;
                }
                Properties.Settings.Default.Save();

                string fullName = GetFullNameByRole(role, linkedId);
                var mw = new MainWindow(fullName, role, linkedId, username);
                mw.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = "Помилка підключення: " + ex.Message;
            }
        }


        private static string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private string DecodeBase64(string encoded)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(encoded)); }
            catch { return ""; }
        }

        private string GetFullNameByRole(string role, int? linkedId)
        {
            if (role == "Admin" || linkedId == null)
                return (string)FindResource("Sys_Admin");

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            string table, idCol;
            switch (role)
            {
                case "Doctor":
                    table = "Doctors"; idCol = "DoctorID"; break;
                case "Receptionist":
                    table = "Receptionist"; idCol = "ReceptionistID"; break;
                default:
                    return (string)FindResource("Unknown_Role");
            }

            using var cmd = new MySqlCommand(
                $"SELECT CONCAT(FirstName,' ',LastName) FROM {table} WHERE {idCol} = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", linkedId);
            var res = cmd.ExecuteScalar();
            return res?.ToString() ?? (string)FindResource("Unknown");
        }
    }
}
