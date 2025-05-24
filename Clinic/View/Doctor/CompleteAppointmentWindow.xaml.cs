using System.Windows;
using Clinic.Models;

namespace Clinic.View.Doctor
{
    public partial class CompleteAppointmentWindow : Window
    {
        private AppointmentModel _appointment;

        public CompleteAppointmentWindow(AppointmentModel appointment)
        {
            InitializeComponent();
            _appointment = appointment;
            NotesBox.Text = appointment.Notes;
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            _appointment.Notes = NotesBox.Text;
            _appointment.Status = "Прийом завершено";

            DialogResult = true;
            Close();
        }
    }
}
