using System.Windows;
using System.Windows.Controls;
using Clinic.Models;
using Clinic.View.Receptionist;
using Clinic.ViewModels;

namespace Clinic.View
{
    public partial class RegisterPatientWindow : UserControl
    {
        public RegisterPatientWindow(RegisterPatientViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is RegisterPatientViewModel vm &&
                ((DataGrid)sender).SelectedItem is Patient patient)
            {
                new SelectDoctorWindow(patient, vm).ShowDialog();

            }
        }

    }
}
