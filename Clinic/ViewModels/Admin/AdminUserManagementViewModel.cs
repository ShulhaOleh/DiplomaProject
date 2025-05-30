using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Clinic.DB;
using Clinic.Models;
using Clinic.View.Admin;
using MySql.Data.MySqlClient;

namespace Clinic.ViewModels.Admin
{
    public class AdminUserManagementViewModel : BaseViewModel
    {
        public ObservableCollection<User> Users { get; } = new();
        public User SelectedUser { get; set; }

        public ICommand DeleteUserCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public string AdminUsername { get; }
        public AdminUserManagementViewModel(string adminUsername)
        {
            AdminUsername = adminUsername;
            LoadUsers();
        }


        public void LoadUsers()
        {
            Users.Clear();

            using var conn = AuthDB.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("SELECT UserID, Username, Role, LinkedID FROM Users WHERE Role != 'Admin'", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int linkedId = reader.IsDBNull("LinkedID") ? 0 : reader.GetInt32("LinkedID");
                string role = reader.GetString("Role");

                var (last, first, father) = GetLinkedNames(role, linkedId);

                Users.Add(new User
                {
                    UserID = reader.GetInt32("UserID"),
                    Username = reader.GetString("Username"),
                    Role = role,
                    LinkedID = linkedId,
                    FirstName = first,
                    LastName = last,
                    FathersName = father
                });
            }
        }

        private (string last, string first, string father) GetLinkedNames(string role, int id)
        {
            if (role != "Doctor" && role != "Receptionist")
                return ("—", "", "");

            string table = role == "Doctor" ? "Doctors" : "Receptionist";
            string idField = role == "Doctor" ? "DoctorID" : "ReceptionistID";

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                $"SELECT LastName, FirstName, FathersName FROM {table} WHERE {idField} = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    reader.GetString("LastName"),
                    reader.GetString("FirstName"),
                    reader.IsDBNull("FathersName") ? "" : reader.GetString("FathersName")
                );
            }

            return ("—", "", "");
        }


        public void DeleteUser(User user, string adminUsername)
        {
            var confirmDialog = new ConfirmAdminPasswordDialog();
            if (confirmDialog.ShowDialog() != true)
                return;

            string enteredPassword = confirmDialog.EnteredPassword;

            using var conn = AuthDB.GetConnection();
            conn.Open();

            var authCmd = new MySqlCommand(
                "SELECT PasswordHash FROM Users WHERE Username = @username AND Role = 'Admin'", conn);
            authCmd.Parameters.AddWithValue("@username", adminUsername);

            var result = authCmd.ExecuteScalar();

            if (result == null)
            {
                MessageBox.Show("Адміністратора не знайдено.");
                return;
            }

            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(enteredPassword);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            string enteredHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            if (result.ToString() != enteredHash)
            {
                MessageBox.Show("Невірний пароль адміністратора.");
                return;
            }

            using var delConn = AuthDB.GetConnection();
            delConn.Open();

            var delCmd = new MySqlCommand("DELETE FROM Users WHERE UserID = @id", delConn);
            delCmd.Parameters.AddWithValue("@id", user.UserID);
            delCmd.ExecuteNonQuery();

            Users.Remove(user);
            MessageBox.Show("Користувача успішно видалено.");
        }




        public void AddUser()
        {
            var dialog = new View.Admin.EditUserDialog(null);
            if (dialog.ShowDialog() == true)
            {
                using var conn = AuthDB.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(
                    "INSERT INTO Users (Username, PasswordHash, Role, LinkedID, CreatedAt) VALUES (@u, MD5(@p), @r, @l, NOW())",
                    conn
                );
                cmd.Parameters.AddWithValue("@u", dialog.Username);
                cmd.Parameters.AddWithValue("@p", dialog.Password);
                cmd.Parameters.AddWithValue("@r", dialog.Role);
                cmd.Parameters.AddWithValue("@l", dialog.LinkedID.HasValue ? dialog.LinkedID : DBNull.Value);

                cmd.ExecuteNonQuery();
                LoadUsers();
            }
        }

        private void EditUser(User user)
        {
            if (user == null) return;

            var dialog = new View.Admin.EditUserDialog(user); 
            if (dialog.ShowDialog() == true)
            {
                using var conn = AuthDB.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(@"
                    UPDATE Users 
                    SET Username = @u, Role = @r, LinkedID = @l
                    " + (string.IsNullOrWhiteSpace(dialog.Password) ? "" : ", PasswordHash = MD5(@p)") + @"
                    WHERE UserID = @id", conn);

                cmd.Parameters.AddWithValue("@u", dialog.Username);
                cmd.Parameters.AddWithValue("@r", dialog.Role);
                cmd.Parameters.AddWithValue("@l", dialog.LinkedID.HasValue ? dialog.LinkedID : DBNull.Value);
                cmd.Parameters.AddWithValue("@id", user.UserID);

                if (!string.IsNullOrWhiteSpace(dialog.Password))
                    cmd.Parameters.AddWithValue("@p", dialog.Password);

                cmd.ExecuteNonQuery();
                LoadUsers();
            }
        }
    }
}
