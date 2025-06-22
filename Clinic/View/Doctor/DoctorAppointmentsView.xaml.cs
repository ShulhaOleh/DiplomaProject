using Clinic.Models;
using System.Windows;
using System.Windows.Controls;
using Clinic.View.Doctor;
using System.Windows.Input;
using Clinic.ViewModels;
using System;
using Clinic.ViewModels.Doctor;

namespace Clinic.View.Doctor
{
    public partial class DoctorAppointmentsView : UserControl
    {
        private Clinic.ViewModels.Doctor.DoctorAppointmentsViewModel _viewModel;

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
                this.DataContext = _viewModel;

                AppointmentService.AppointmentAdded -= OnAppointmentAddedHandler;
                AppointmentService.AppointmentAdded += OnAppointmentAddedHandler;
            }
        }

        private void OnAppointmentAddedHandler(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel?.LoadAppointmentsFromDatabase();
            });
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg || dg.SelectedItem is not Appointment selected)
                return;

            if (selected.Status == "Прийом завершено" || selected.Status == "Пацієнт не з’явився")
                return;

            var dialog = new CompleteAppointmentWindow(selected);
            bool? result = dialog.ShowDialog();

            if (result == true && _viewModel != null)
            {
                _viewModel.CompleteAppointment(selected);

                string msg = selected.Status switch
                {
                    "Прийом завершено" => "Прийом успішно завершено!",
                    "Пацієнт не з’явився" => "Пацієнта позначено як «не з’явився».",
                    _ => "Статус оновлено."
                };

                MessageBox.Show(msg, "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _viewModel?.LoadAppointmentsFromDatabase();
        }


    }

}
