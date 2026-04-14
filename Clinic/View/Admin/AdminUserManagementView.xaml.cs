using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clinic.Models;
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
                vm.DeleteUser(vm.SelectedUser);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminUserManagementViewModel vm)
            {
                vm.AddUser();
            }
        }





        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminUserManagementViewModel vm)
                vm.EditUser(vm.SelectedUser);
        }

        private void UserGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AdminUserManagementViewModel vm &&
                (sender as DataGrid)?.SelectedItem is User user)
                vm.EditUser(user);
        }


    }
}
