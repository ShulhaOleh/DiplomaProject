using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
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
            using var cmd = new MySqlCommand(
                "SELECT UserID, Username, Role, LinkedID FROM Users WHERE Role <> 'Admin'",
                conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var u = new User
                {
                    UserID = rdr.GetInt32("UserID"),
                    Username = rdr.GetString("Username"),
                    Role = rdr.GetString("Role"),
                    LinkedID = rdr.IsDBNull("LinkedID")
                                 ? (int?)null
                                 : rdr.GetInt32("LinkedID")
                };

                var (last, first, father) = GetLinkedNames(u.Role, u.LinkedID);
                u.LastName = last;
                u.FirstName = first;
                u.FathersName = father;

                Users.Add(u);
            }
        }

        private (string last, string first, string father) GetLinkedNames(string role, int? id)
        {
            if (id == null || (role != "Doctor" && role != "Receptionist"))
                return ("—", "", "");

            string table = role == "Doctor" ? "Doctors" : "Receptionist";
            string idField = role == "Doctor" ? "DoctorID" : "ReceptionistID";

            using var conn = ClinicDB.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                $"SELECT LastName, FirstName, FathersName FROM {table} WHERE {idField}=@id",
                conn);
            cmd.Parameters.AddWithValue("@id", id.Value);

            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read()) return ("—", "", "");

            var last = rdr.GetString("LastName");
            var first = rdr.GetString("FirstName");
            var father = rdr.IsDBNull("FathersName") ? "" : rdr.GetString("FathersName");
            return (last, first, father);
        }

        public void DeleteUser(User user)
        {
            if (user == null) return;

            var dlg = new ConfirmAdminPasswordDialog();
            if (dlg.ShowDialog() != true) return;

            using var conn = AuthDB.GetConnection();
            conn.Open();
            using var authCmd = new MySqlCommand(
                "SELECT PasswordHash FROM Users WHERE Username=@u AND Role='Admin'",
                conn);
            authCmd.Parameters.AddWithValue("@u", AdminUsername);

            var storedHash = authCmd.ExecuteScalar() as string;
            if (storedHash == null)
            {
                MessageBox.Show("Адміністратора не знайдено.");
                return;
            }

            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(dlg.EnteredPassword));
            var entered = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            if (entered != storedHash)
            {
                MessageBox.Show("Невірний пароль адміністратора.");
                return;
            }

            using var delCmd = new MySqlCommand(
                "DELETE FROM Users WHERE UserID=@id", conn);
            delCmd.Parameters.AddWithValue("@id", user.UserID);

            try
            {
                delCmd.ExecuteNonQuery();
                LoadUsers();
                MessageBox.Show("Користувача успішно видалено.");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Помилка при видаленні: {ex.Message}");
            }
        }

        public void AddUser()
        {
            var dlg = new EditUserDialog(null);
            if (dlg.ShowDialog() != true) return;

            using var conn = AuthDB.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO Users (Username,PasswordHash,Role,LinkedID,CreatedAt) " +
                "VALUES (@u,MD5(@p),@r,@l,NOW())",
                conn);
            cmd.Parameters.AddWithValue("@u", dlg.Username);
            cmd.Parameters.AddWithValue("@p", dlg.Password);
            cmd.Parameters.AddWithValue("@r", dlg.Role);
            cmd.Parameters.AddWithValue("@l", dlg.LinkedID ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();
            LoadUsers();
        }

        public void EditUser(User user)
        {
            if (user == null) return;

            var dlg = new EditUserDialog(user);
            if (dlg.ShowDialog() != true) return;

            using var conn = AuthDB.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE Users SET Username=@u,Role=@r,LinkedID=@l" +
                (string.IsNullOrWhiteSpace(dlg.Password) ? "" : ",PasswordHash=MD5(@p)") +
                " WHERE UserID=@id",
                conn);

            cmd.Parameters.AddWithValue("@u", dlg.Username);
            cmd.Parameters.AddWithValue("@r", dlg.Role);
            cmd.Parameters.AddWithValue("@l", dlg.LinkedID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", user.UserID);
            if (!string.IsNullOrWhiteSpace(dlg.Password))
                cmd.Parameters.AddWithValue("@p", dlg.Password);

            cmd.ExecuteNonQuery();
            LoadUsers();
        }
    }
}
