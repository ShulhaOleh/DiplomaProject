using Clinic.Models;
using System.Windows;
using System.Windows.Controls;
using Clinic.View.Doctor;
using System.Windows.Input;

namespace Clinic.View.Doctor
{
    public partial class DoctorAppointmentsView : UserControl
    {
        public DoctorAppointmentsView()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg || dg.SelectedItem is not AppointmentModel selected)
                return;

            if (selected.Status == "Прийом завершено")
                return;

            var dialog = new CompleteAppointmentWindow(selected);
            bool? result = dialog.ShowDialog();

            if (result == true && DataContext is Clinic.ViewModels.Doctor.DoctorAppointmentsViewModel vm)
            {
                vm.CompleteAppointment(selected);
                vm.LoadAppointmentsFromDatabase();

                MessageBox.Show("Прийом успішно завершено!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


    }
}
