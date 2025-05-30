using System.Windows;

namespace Clinic.View.Admin
{
    public partial class ConfirmAdminPasswordDialog : Window
    {
        public string EnteredPassword => PasswordBox.Password;

        public ConfirmAdminPasswordDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
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
