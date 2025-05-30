using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Clinic.DB;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows;
using System;

namespace Clinic.ViewModels
{

    public class LoginViewModel : INotifyPropertyChanged
    {
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            LoadSavedCredentials();
        }

        private string _username;
        private string _password;
        private bool _rememberMe;
        private string _message;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set { _rememberMe = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public void LoadSavedCredentials()
        {
            string encodedLogin = Properties.Settings.Default.SavedUsername;
            string encodedPass = Properties.Settings.Default.SavedPassword;

            if (!string.IsNullOrEmpty(encodedLogin))
                Username = DecodeBase64(encodedLogin);
            if (!string.IsNullOrEmpty(encodedPass))
                Password = DecodeBase64(encodedPass);
            RememberMe = Properties.Settings.Default.RememberMe;
        }

        public void SaveCredentials()
        {
            Properties.Settings.Default.SavedUsername = EncodeBase64(Username);
            Properties.Settings.Default.SavedPassword = EncodeBase64(Password);
            Properties.Settings.Default.RememberMe = RememberMe;
            Properties.Settings.Default.Save();
        }

        private string EncodeBase64(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        private string DecodeBase64(string input)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(input)); }
            catch { return string.Empty; }
        }

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Message = App.Current.TryFindResource("EmptyLoginFieldsMessage").ToString();
                return;
            }

            using var conn = AuthDB.GetConnection();
            conn.Open();

            string hash = GetMd5Hash(Password);

            var cmd = new MySqlCommand("SELECT Role, LinkedID FROM Users WHERE Username = @u AND PasswordHash = @p", conn);
            cmd.Parameters.AddWithValue("@u", Username);
            cmd.Parameters.AddWithValue("@p", hash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string role = reader.GetString("Role");
                int? linkedId = null;

                if (!reader.IsDBNull(reader.GetOrdinal("LinkedID")))
                    linkedId = reader.GetInt32(reader.GetOrdinal("LinkedID"));



                SaveCredentials();

                App.Current.Dispatcher.Invoke(() =>
                {
                    string fullName = GetFullNameByRole(role, linkedId);
                    if (fullName != null)
                    {
                        var mainWindow = new View.MainWindow(fullName, role, linkedId, Username);

                        mainWindow.Show();
                        CloseLoginWindow();
                    }
                    else
                        Message = App.Current.TryFindResource("NoRoleMessage").ToString();
                });
            }
            else
                Message = App.Current.TryFindResource("IncorrectPasswordOrLoginMessage").ToString();
        }

        public static string GetFullNameByRole(string role, int? linkedId)
        {
            if (role == "Admin" || linkedId == null)
                return "Системний адміністратор";

            using (var conn = Clinic.DB.ClinicDB.GetConnection())
            {
                conn.Open();

                string table, idColumn;

                switch (role)
                {
                    case "Doctor":
                        table = "Doctors";
                        idColumn = "DoctorID";
                        break;
                    case "Receptionist":
                        table = "Receptionist";
                        idColumn = "ReceptionistID";
                        break;
                    default:
                        return "Невідома роль";
                }

                var cmd = new MySqlCommand($"SELECT CONCAT(FirstName, ' ', LastName) FROM {table} WHERE {idColumn} = @id", conn);
                cmd.Parameters.AddWithValue("@id", linkedId);

                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? "Невідомо";
            }
        }


        private static string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private void CloseLoginWindow()
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == "Login")
                {
                    window.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
