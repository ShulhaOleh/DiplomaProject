using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Clinic.Models;
using Clinic.DB;
using MySql.Data.MySqlClient;
using System.Data;

namespace Clinic.View.Admin
{
    public partial class EditUserDialog : Window
    {
        public string Username => UsernameBox.Text.Trim();
        public string Role => (RoleBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        public string Password => PasswordBox.Password;
        public string FirstName => FirstNameBox.Text.Trim();
        public string LastName => LastNameBox.Text.Trim();
        public string FathersName => FathersNameBox.Text.Trim();
        public string Phone => PhoneBox.Text.Trim();

        private readonly bool _isEdit;
        private readonly int? _existingLinkedId;

        public int? LinkedID { get; private set; }

        public EditUserDialog(User user)
        {
            InitializeComponent();
            _isEdit = user != null;

            if (_isEdit)
            {
                Title = "Редагування користувача";
                UsernameBox.Text = user.Username;
                _existingLinkedId = user.LinkedID;
                PasswordLabel.Text = "Новий пароль:";

                RoleBox.SelectedItem = RoleBox.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == user.Role);

                if (user.Role is "Doctor" or "Receptionist" && user.LinkedID.HasValue)
                {
                    LoadProfileData(user.Role, user.LinkedID.Value);
                    LinkedID = user.LinkedID.Value;
                }
            }
            else
            {
                Title = "Додавання користувача";
                RoleBox.SelectedIndex = 0;
                _existingLinkedId = null;
                LinkedID = null;
                PasswordLabel.Text = "Пароль:";
            }
        }

        private void LoadProfileData(string role, int id)
        {
            string table = role == "Doctor" ? "Doctors" : "Receptionist";
            string idField = role == "Doctor" ? "DoctorID" : "ReceptionistID";

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                $"SELECT LastName, FirstName, FathersName, PhoneNumber FROM {table} WHERE {idField} = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                LastNameBox.Text = reader.GetString("LastName");
                FirstNameBox.Text = reader.GetString("FirstName");
                FathersNameBox.Text = reader.IsDBNull("FathersName") ? "" : reader.GetString("FathersName");
                PhoneBox.Text = reader.IsDBNull("PhoneNumber") ? "" : reader.GetString("PhoneNumber");
            }
        }

        private void RoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEdit)
            {
                LastNameBox.Text = "";
                FirstNameBox.Text = "";
                FathersNameBox.Text = "";
                PhoneBox.Text = "";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Введіть логін.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Role))
            {
                MessageBox.Show("Оберіть роль.");
                return;
            }

            if ((Role == "Doctor" || Role == "Receptionist") &&
                (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName)))
            {
                MessageBox.Show("Введіть прізвище та ім’я для профілю.");
                return;
            }

            if (_isEdit)
            {
                if (Role is "Doctor" or "Receptionist" && LinkedID.HasValue)
                {
                    string table = Role == "Doctor" ? "Doctors" : "Receptionist";
                    string idField = Role == "Doctor" ? "DoctorID" : "ReceptionistID";

                    using var conn = ClinicDB.GetConnection();
                    conn.Open();

                    var cmd = new MySqlCommand($@"
                        UPDATE {table}
                        SET LastName = @last, FirstName = @first, FathersName = @father, PhoneNumber = @phone
                        WHERE {idField} = @id", conn);

                    cmd.Parameters.AddWithValue("@last", LastName);
                    cmd.Parameters.AddWithValue("@first", FirstName);
                    cmd.Parameters.AddWithValue("@father", string.IsNullOrWhiteSpace(FathersName) ? DBNull.Value : FathersName);
                    cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(Phone) ? DBNull.Value : Phone);
                    cmd.Parameters.AddWithValue("@id", LinkedID.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                if (Role is "Doctor" or "Receptionist")
                {
                    string table = Role == "Doctor" ? "Doctors" : "Receptionist";
                    string cmdText;

                    if (Role == "Doctor")
                    {
                        cmdText = @"
                            INSERT INTO Doctors (LastName, FirstName, FathersName, PhoneNumber, DateOfBirth, Specialty, BreakHour)
                            VALUES (@last, @first, @father, @phone, CURDATE(), 'ЛОР', '13:00:00');
                            SELECT LAST_INSERT_ID();";
                    }
                    else
                    {
                        cmdText = @"
                            INSERT INTO Receptionist (LastName, FirstName, FathersName, PhoneNumber)
                            VALUES (@last, @first, @father, @phone);
                            SELECT LAST_INSERT_ID();";
                    }

                    using var conn = ClinicDB.GetConnection();
                    conn.Open();

                    var cmd = new MySqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@last", LastName);
                    cmd.Parameters.AddWithValue("@first", FirstName);
                    cmd.Parameters.AddWithValue("@father", string.IsNullOrWhiteSpace(FathersName) ? DBNull.Value : FathersName);
                    cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(Phone) ? DBNull.Value : Phone);

                    LinkedID = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
