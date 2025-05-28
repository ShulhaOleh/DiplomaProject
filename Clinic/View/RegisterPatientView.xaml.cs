using System.Windows.Controls;
using Clinic.Models;
using Clinic.ViewModels;

namespace Clinic.View
{
    public partial class RegisterPatientWindow : UserControl
    {
        public RegisterPatientWindow() : this(0) { }

        public RegisterPatientWindow(int doctorId)
        {
            InitializeComponent();
            DataContext = new RegisterPatientViewModel(doctorId);
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is RegisterPatientViewModel vm &&
                ((DataGrid)sender).SelectedItem is Patient patient)
            {
                vm.RegisterAppointment(patient);
            }
        }
    }
}
