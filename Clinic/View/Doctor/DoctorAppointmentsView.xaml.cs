using Clinic.Models;
using System;
using System.Linq;
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
        private readonly string _statusCompleted;
        private readonly string _statusNoShow;
        private readonly string _msgCompleted;
        private readonly string _msgNoShow;
        private readonly string _msgUpdated;
        private readonly string _titleDone;

        public DoctorAppointmentsView()
        {
            InitializeComponent();

            // подгружаем переводы из ресурсов
            _statusCompleted = (string)Application.Current.TryFindResource("Status_Completed");
            _statusNoShow = (string)Application.Current.TryFindResource("Status_NoShow");
            _msgCompleted = (string)Application.Current.TryFindResource("Msg_CompletedSuccess");
            _msgNoShow = (string)Application.Current.TryFindResource("Msg_NoShowSuccess");
            _msgUpdated = (string)Application.Current.TryFindResource("Msg_StatusUpdated");
            _titleDone = (string)Application.Current.TryFindResource("Title_Done");

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

            if (selected.Status == _statusCompleted
             || selected.Status == _statusNoShow)
                return;

            var dialog = new CompleteAppointmentWindow(selected);
            bool? result = dialog.ShowDialog();

            if (result == true && _viewModel != null)
            {
                _viewModel.CompleteAppointment(selected);

                string msg = selected.Status switch
                {
                    var s when s == _statusCompleted => _msgCompleted,
                    var s when s == _statusNoShow => _msgNoShow,
                    _ => _msgUpdated
                };

                MessageBox.Show(msg, _titleDone, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _viewModel?.LoadAppointmentsFromDatabase();
        }
    }
}
