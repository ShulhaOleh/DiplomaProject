using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clinic.DB;
using Clinic.Models;
using MySql.Data.MySqlClient;

namespace Clinic.View.Admin
{
    public partial class EditUserDialog : Window
    {
        private readonly bool _isEdit;
        private readonly User _user;

        // Публічні властивості для ViewModel
        public string Username => UsernameBox.Text.Trim();
        public string Role => (RoleBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        public string Password => PasswordBox.Password;
        public string LastName => LastNameBox.Text.Trim();
        public string FirstName => FirstNameBox.Text.Trim();
        public string FathersName => FathersNameBox.Text.Trim();
        public string Phone => PhoneBox.Text.Trim();
        public int? LinkedID { get; private set; }

        public EditUserDialog(User user)
        {
            InitializeComponent();
            _user = user;
            _isEdit = user != null;

            // Прив’язуємо стартове значення в випадку редагування
            if (_isEdit)
            {
                Title = "Редагування користувача";
                UsernameBox.Text = _user.Username;
                PasswordLabel.Text = "Новий пароль:";
                LinkedID = _user.LinkedID;

                // Встановити роль
                var existingRole = RoleBox.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == _user.Role);
                if (existingRole != null)
                    RoleBox.SelectedItem = existingRole;

                // Завантажити дані профілю
                if (_user.LinkedID.HasValue)
                    LoadProfileData(_user.Role, _user.LinkedID.Value);
            }
            else
            {
                Title = "Додавання користувача";
                PasswordLabel.Text = "Пароль:";
            }

            // За замовчуванням перша роль
            if (RoleBox.SelectedIndex < 0)
                RoleBox.SelectedIndex = 0;
        }

        private void RoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEdit)
            {
                LastNameBox.Clear();
                FirstNameBox.Clear();
                FathersNameBox.Clear();
                PhoneBox.Value = null;  // очищаємо маску
            }
        }

        private void LoadProfileData(string role, int linkedId)
        {
            var table = role == "Doctor" ? "Doctors" : "Receptionist";
            var idField = role == "Doctor" ? "DoctorID" : "ReceptionistID";

            using var conn = ClinicDB.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand($@"
                SELECT LastName, FirstName, FathersName, PhoneNumber
                  FROM {table}
                 WHERE {idField} = @id", conn);
            cmd.Parameters.AddWithValue("@id", linkedId);

            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                LastNameBox.Text = r.IsDBNull("LastName") ? "" : r.GetString("LastName");
                FirstNameBox.Text = r.IsDBNull("FirstName") ? "" : r.GetString("FirstName");
                FathersNameBox.Text = r.IsDBNull("FathersName") ? "" : r.GetString("FathersName");
                PhoneBox.Text = r.IsDBNull("PhoneNumber") ? "" : r.GetString("PhoneNumber");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Базова валідація
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Введіть логін."); return;
            }
            if (string.IsNullOrWhiteSpace(Role))
            {
                MessageBox.Show("Оберіть роль."); return;
            }
            if (string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(FirstName))
            {
                MessageBox.Show("Введіть прізвище та ім’я."); return;
            }
            // Перевірка заповнення маски: "+380 (xx) xxx-xxxx"
            if (PhoneBox.MaskedTextProvider != null &&
                !PhoneBox.MaskedTextProvider.MaskCompleted)
            {
                MessageBox.Show("Телефон має бути у форматі +380 (xx) xxx-xxxx.");
                return;
            }

            if (_isEdit)
                UpdateProfileData();
            else
                InsertProfileData();

            DialogResult = true;
            Close();
        }

        private void UpdateProfileData()
        {
            if (!_user.LinkedID.HasValue) return;

            var table = Role == "Doctor" ? "Doctors" : "Receptionist";
            var idField = Role == "Doctor" ? "DoctorID" : "ReceptionistID";

            using var conn = ClinicDB.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand($@"
                UPDATE {table}
                   SET LastName    = @last,
                       FirstName   = @first,
                       FathersName = @father,
                       PhoneNumber = @phone
                 WHERE {idField}   = @id", conn);
            cmd.Parameters.AddWithValue("@last", LastName);
            cmd.Parameters.AddWithValue("@first", FirstName);
            cmd.Parameters.AddWithValue("@father", string.IsNullOrWhiteSpace(FathersName)
                                                  ? DBNull.Value
                                                  : FathersName);
            cmd.Parameters.AddWithValue("@phone", Phone);
            cmd.Parameters.AddWithValue("@id", _user.LinkedID.Value);
            cmd.ExecuteNonQuery();
        }

        private void InsertProfileData()
        {
            var table = Role == "Doctor" ? "Doctors" : "Receptionist";
            var columns = Role == "Doctor"
                ? "LastName, FirstName, FathersName, PhoneNumber, DateOfBirth, Specialty, BreakHour"
                : "LastName, FirstName, FathersName, PhoneNumber";
            var values = Role == "Doctor"
                ? "@last, @first, @father, @phone, CURDATE(), 'ЛОР', '13:00:00'"
                : "@last, @first, @father, @phone";

            using var conn = ClinicDB.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand($@"
                INSERT INTO {table} ({columns})
                VALUES ({values});
                SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@last", LastName);
            cmd.Parameters.AddWithValue("@first", FirstName);
            cmd.Parameters.AddWithValue("@father", string.IsNullOrWhiteSpace(FathersName)
                                                  ? DBNull.Value
                                                  : FathersName);
            cmd.Parameters.AddWithValue("@phone", Phone);
            LinkedID = Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
