using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Clinic.DB;
using Clinic.Models;
using MySql.Data.MySqlClient;

namespace Clinic.View.Admin
{
    public partial class EditUserDialog : Window
    {
        private readonly bool _isEdit;
        private readonly User _user;
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

            RoleBox.SelectedIndex = 0;

            if (_isEdit)
            {
                Title = "Редагування користувача";
                UsernameBox.Text = user.Username;
                PasswordLabel.Text = "Новий пароль:";

                LinkedID = user.LinkedID;

                if (LinkedID.HasValue)
                    LoadProfileData(user.Role, LinkedID.Value);

                var item = RoleBox.Items
                                  .Cast<ComboBoxItem>()
                                  .FirstOrDefault(i => i.Content.ToString() == user.Role);
                if (item != null) RoleBox.SelectedItem = item;
            }
            else
            {
                Title = "Додавання користувача";
                PasswordLabel.Text = "Пароль:";
                LinkedID = null;
            }

            RoleBox.SelectionChanged += RoleBox_SelectionChanged;
        }

        private void RoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEdit)
            {
                LastNameBox.Clear();
                FirstNameBox.Clear();
                FathersNameBox.Clear();
                PhoneBox.Clear();
            }
        }

        private void LoadProfileData(string role, int linkedId)
        {
            string table = role == "Doctor" ? "Doctors" : "Receptionist";
            string idField = role == "Doctor" ? "DoctorID" : "ReceptionistID";

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                $"SELECT LastName, FirstName, FathersName, PhoneNumber FROM {table} WHERE {idField}=@id",
                conn);
            cmd.Parameters.AddWithValue("@id", linkedId);

            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read()) return;

            LastNameBox.Text = rdr.IsDBNull("LastName") ? "" : rdr.GetString("LastName");
            FirstNameBox.Text = rdr.IsDBNull("FirstName") ? "" : rdr.GetString("FirstName");
            FathersNameBox.Text = rdr.IsDBNull("FathersName") ? "" : rdr.GetString("FathersName");
            PhoneBox.Text = rdr.IsDBNull("PhoneNumber") ? "" : rdr.GetString("PhoneNumber");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Role) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(FirstName))
            {
                MessageBox.Show("Заповніть усі обов’язкові поля.");
                return;
            }

            if (_isEdit)
            {
                UpdateProfileData();
            }
            else
            {
                InsertProfileData();
            }

            DialogResult = true;
            Close();
        }

        private void UpdateProfileData()
        {
            if (!LinkedID.HasValue) return;

            string table = Role == "Doctor" ? "Doctors" : "Receptionist";
            string idField = Role == "Doctor" ? "DoctorID" : "ReceptionistID";

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
            cmd.Parameters.AddWithValue("@father", string.IsNullOrWhiteSpace(FathersName) ? DBNull.Value : FathersName);
            cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(Phone) ? DBNull.Value : Phone);
            cmd.Parameters.AddWithValue("@id", LinkedID.Value);

            cmd.ExecuteNonQuery();
        }

        private void InsertProfileData()
        {
            string table = Role == "Doctor" ? "Doctors" : "Receptionist";
            string columns = Role == "Doctor"
                ? "LastName,FirstName,FathersName,PhoneNumber,DateOfBirth,Specialty,BreakHour"
                : "LastName,FirstName,FathersName,PhoneNumber";
            string values = Role == "Doctor"
                ? "@last,@first,@father,@phone,CURDATE(),'ЛОР','13:00:00'; SELECT LAST_INSERT_ID();"
                : "@last,@first,@father,@phone; SELECT LAST_INSERT_ID();";

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                $"INSERT INTO {table} ({columns}) VALUES ({values})",
                conn);

            cmd.Parameters.AddWithValue("@last", LastName);
            cmd.Parameters.AddWithValue("@first", FirstName);
            cmd.Parameters.AddWithValue("@father", string.IsNullOrWhiteSpace(FathersName) ? DBNull.Value : FathersName);
            cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(Phone) ? DBNull.Value : Phone);

            LinkedID = Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
