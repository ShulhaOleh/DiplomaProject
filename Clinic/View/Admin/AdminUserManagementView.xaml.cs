using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clinic.Models;
using Clinic.View.Admin;
using Clinic.DB;
using MySql.Data.MySqlClient;
using System;
using Clinic.ViewModels.Admin;

namespace Clinic.View.Admin
{
    public partial class AdminUserManagementView : UserControl
    {

        public AdminUserManagementView()
        {
            InitializeComponent();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminUserManagementViewModel vm && vm.SelectedUser != null)
            {
                vm.DeleteUser(vm.SelectedUser, vm.AdminUsername);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminUserManagementViewModel vm)
            {
                vm.AddUser();
            }
        }





        private void UserGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as DataGrid)?.SelectedItem is User user)
            {
                var dialog = new EditUserDialog(user);
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
                    cmd.Parameters.AddWithValue("@l", dialog.LinkedID.HasValue ? dialog.LinkedID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", user.UserID);

                    if (!string.IsNullOrWhiteSpace(dialog.Password))
                        cmd.Parameters.AddWithValue("@p", dialog.Password);

                    cmd.ExecuteNonQuery();
                }

                if (DataContext is Clinic.ViewModels.Admin.AdminUserManagementViewModel vm)
                {
                    vm.LoadUsers();
                }
            }
        }


    }
}
