using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Clinic.DB;
using MySql.Data.MySqlClient;

namespace Clinic.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        public int UserId { get; }
        public string UserRole { get; }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _dateOfBirth;
        public string DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        private bool _isPhoneEditable;
        public bool IsPhoneEditable
        {
            get => _isPhoneEditable;
            set { _isPhoneEditable = value; OnPropertyChanged(); }
        }


        public ICommand SaveCommand { get; }

        public ProfileViewModel(int userId, string userRole)
        {
            UserId = userId;
            UserRole = userRole;
            LoadUserInfo();
            SaveCommand = new RelayCommand(Save, () => IsPhoneEditable);
        }

        private void LoadUserInfo()
        {
            using var conn = ClinicDB.GetConnection();
            conn.Open();

            string query = UserRole == "Doctor"
                ? "SELECT LastName, FirstName, FathersName, DateOfBirth, PhoneNumber FROM Doctors WHERE DoctorID = @id"
                : "SELECT LastName, FirstName, FathersName, NULL AS DateOfBirth, PhoneNumber FROM Receptionist WHERE ReceptionistID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", UserId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string last = reader.IsDBNull(0) ? "" : reader.GetString(0);
                string first = reader.IsDBNull(1) ? "" : reader.GetString(1);
                string father = reader.IsDBNull(2) ? "" : reader.GetString(2);
                FullName = $"{last} {first} {father}".Trim();

                DateOfBirth = reader.IsDBNull(3) ? "" : reader.GetDateTime(3).ToString("dd.MM.yyyy");
                PhoneNumber = reader.IsDBNull(4) ? "" : reader.GetString(4);
            }
        }



        public bool VerifyPassword(string plainPassword)
        {
            using var conn = AuthDB.GetConnection();
            conn.Open();

            string hash = Hash(plainPassword);

            var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE LinkedID = @id AND Role = @role AND PasswordHash = @hash", conn);
            cmd.Parameters.AddWithValue("@id", UserId);
            cmd.Parameters.AddWithValue("@role", UserRole);
            cmd.Parameters.AddWithValue("@hash", hash);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private string Hash(string input)
        {
            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private bool PhoneExists(string phone)
        {
            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var query = UserRole == "Doctor"
                ? "SELECT COUNT(*) FROM Doctors WHERE PhoneNumber = @phone AND DoctorID != @id"
                : "SELECT COUNT(*) FROM Receptionist WHERE PhoneNumber = @phone AND ReceptionistID != @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@id", UserId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private void Save()
        {
            if (PhoneExists(PhoneNumber))
            {
                MessageBox.Show("Користувач з таким номером вже існує. Введіть інший номер.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(PhoneNumber, @"^\+380\d{9}$"))
            {
                MessageBox.Show("Номер телефону має бути у форматі +380XXXXXXXXX.", "Невірний формат", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            string query = UserRole == "Doctor"
    ? "SELECT LastName, FirstName, FathersName, DateOfBirth, PhoneNumber FROM Doctors WHERE DoctorID = @id"
    : "SELECT LastName, FirstName, FathersName, NULL AS DateOfBirth, PhoneNumber FROM Receptionist WHERE ReceptionistID = @id";


            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@phone", PhoneNumber);
            cmd.Parameters.AddWithValue("@id", UserId);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Номер телефону успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            IsPhoneEditable = false;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
