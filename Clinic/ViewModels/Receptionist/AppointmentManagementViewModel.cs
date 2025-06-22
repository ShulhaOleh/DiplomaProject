using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Clinic.DB;
using Clinic.Models;
using MySql.Data.MySqlClient;

namespace Clinic.ViewModels.Receptionist
{
    public class AppointmentManagementViewModel : BaseViewModel
    {
        public ObservableCollection<Appointment> TodayAppointments { get; }
            = new ObservableCollection<Appointment>();

        public Appointment SelectedAppointment { get; set; }

        public ICommand MarkNoShowCommand { get; }
        public ICommand ReloadAppointmentsCommand { get; }

        private readonly int _receptionistId;

        public AppointmentManagementViewModel(int receptionistId)
        {
            _receptionistId = receptionistId;

            MarkNoShowCommand = new RelayCommand<Appointment>(MarkAsNoShow);
            ReloadAppointmentsCommand = new RelayCommand(LoadTodayAppointments);

            LoadTodayAppointments();
        }

        public void LoadTodayAppointments()
        {
            TodayAppointments.Clear();
            var today = DateTime.Today;

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT 
                    a.AppointmentID,
                    a.PatientID,
                    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
                    a.AppointmentDate,
                    a.Status
                FROM Appointments a
                JOIN Patients p ON p.PatientID = a.PatientID
                WHERE DATE(a.AppointmentDate) = @today
                ORDER BY a.AppointmentDate ASC", conn);
            cmd.Parameters.AddWithValue("@today", today);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TodayAppointments.Add(new Appointment
                {
                    AppointmentID = reader.GetInt32("AppointmentID"),
                    PatientID = reader.GetInt32("PatientID"),
                    PatientName = reader.GetString("PatientName"),
                    AppointmentDate = reader.GetDateTime("AppointmentDate"),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status"))
                             ? (string)Application.Current.FindResource("Status_Expected")
                             : reader.GetString("Status")
                });
            }
        }

        private void MarkAsNoShow(Appointment appt)
        {
            if (appt == null) return;

            var completed = (string)Application.Current.FindResource("Status_Completed");
            var noShow = (string)Application.Current.FindResource("Status_NoShow");

            if (appt.Status == completed || appt.Status == noShow)
            {
                MessageBox.Show(
                    (string)Application.Current.FindResource("Msg_AlreadyHandled"),
                    (string)Application.Current.FindResource("Title_Done"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "UPDATE Appointments SET Status = @status WHERE AppointmentID = @id", conn);
            cmd.Parameters.AddWithValue("@status", noShow);
            cmd.Parameters.AddWithValue("@id", appt.AppointmentID);
            cmd.ExecuteNonQuery();

            MessageBox.Show(
                (string)Application.Current.FindResource("Msg_NoShowSuccess"),
                (string)Application.Current.FindResource("Title_Done"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadTodayAppointments();
        }
    }
}
