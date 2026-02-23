using Clinic.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clinic.ViewModels;
using Clinic.ViewModels.Doctor;

namespace Clinic.View.Doctor
{
    public partial class DoctorAppointmentsView : UserControl
    {
        private DoctorAppointmentsViewModel _viewModel;

        public DoctorAppointmentsView()
        {
            InitializeComponent();
            Loaded += DoctorAppointmentsView_Loaded;
        }

        private void DoctorAppointmentsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentDoctorId is int doctorId)
            {
                _viewModel = new DoctorAppointmentsViewModel(doctorId);
                DataContext = _viewModel;

                AppointmentService.AppointmentAdded -= OnAppointmentAddedHandler;
                AppointmentService.AppointmentAdded += OnAppointmentAddedHandler;
            }
        }

        private void OnAppointmentAddedHandler(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => _viewModel?.LoadAppointmentsFromDatabase());
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg || dg.SelectedItem is not Appointment selected)
                return;

            if (selected.IsCompleted || selected.IsNoShow)
                return;

            var dialog = new CompleteAppointmentWindow(selected);
            bool? result = dialog.ShowDialog();

            if (result == true && _viewModel != null)
            {
                _viewModel.CompleteAppointment(selected);

                string msg = selected.IsCompleted
                    ? (string)Application.Current.FindResource("Msg_CompletedSuccess")
                    : selected.IsNoShow
                        ? (string)Application.Current.FindResource("Msg_NoShowSuccess")
                        : (string)Application.Current.FindResource("Msg_StatusUpdated");

                MessageBox.Show(msg,
                    (string)Application.Current.FindResource("Title_Done"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _viewModel?.LoadAppointmentsFromDatabase();
        }
    }
}
