using Clinic.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Clinic.ViewModels.Doctor
{
    public class DoctorAppointmentsViewModel : BaseViewModel
    {
        public ObservableCollection<Appointment> TodayAppointments { get; } = new();
        public ObservableCollection<Appointment> UpcomingAppointments { get; } = new();
        public ObservableCollection<Appointment> PastAppointments { get; } = new();

        public ICommand RegisterPatientCommand { get; }

        private readonly int _doctorId;
        private readonly string _statusExpected;
        private readonly string _statusNoShow;
        private readonly string _statusCompleted;

        public DoctorAppointmentsViewModel(int doctorId)
        {
            _doctorId = doctorId;
            _statusExpected = (string)Application.Current.FindResource("Status_Expected");
            _statusNoShow = (string)Application.Current.FindResource("Status_NoShow");
            _statusCompleted = (string)Application.Current.FindResource("Status_Completed");

            RegisterPatientCommand = new RelayCommand<object>(RegisterPatient);
            LoadAppointmentsFromDatabase();
            AppointmentService.AppointmentAdded += OnAppointmentAdded;
        }

        private void OnAppointmentAdded(object sender, EventArgs e)
            => Application.Current.Dispatcher.Invoke(LoadAppointmentsFromDatabase);

        private void RegisterPatient(object obj)
        {
            if (obj is not string fullName || string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Введіть повне ім’я пацієнта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            AppointmentService.RegisterAppointment(fullName, _doctorId, DateTime.Today, "Doctor");
        }

        public void LoadAppointmentsFromDatabase()
        {
            var appointments = new List<Appointment>();
            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();

            const string sql = @"
                SELECT 
                    a.AppointmentID,
                    p.PatientID,
                    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
                    a.AppointmentDate,
                    a.Status,
                    a.Notes
                FROM Appointments a
                JOIN Patients p ON a.PatientID = p.PatientID
                WHERE a.DoctorID = @doctorId
                ORDER BY a.AppointmentDate ASC;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@doctorId", _doctorId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                appointments.Add(new Appointment
                {
                    AppointmentID = reader.GetInt32("AppointmentID"),
                    PatientID = reader.GetInt32("PatientID"),
                    PatientName = reader.GetString("PatientName"),
                    AppointmentDate = reader.GetDateTime("AppointmentDate"),
                    Status = reader.IsDBNull("Status") ? _statusExpected : reader.GetString("Status"),
                    Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes")
                });
            }

            foreach (var ap in appointments
                     .Where(a => a.AppointmentDate.Date < DateTime.Today && a.Status == _statusExpected))
            {
                ap.Status = _statusNoShow;
                using var upd = new MySqlCommand(
                    "UPDATE Appointments SET Status = @status WHERE AppointmentID = @id", conn);
                upd.Parameters.AddWithValue("@status", ap.Status);
                upd.Parameters.AddWithValue("@id", ap.AppointmentID);
                upd.ExecuteNonQuery();
            }

            var today = DateTime.Today;
            TodayAppointments.Clear();
            foreach (var a in appointments.Where(a => a.AppointmentDate.Date == today))
                TodayAppointments.Add(a);

            UpcomingAppointments.Clear();
            foreach (var a in appointments.Where(a => a.AppointmentDate.Date > today))
                UpcomingAppointments.Add(a);

            PastAppointments.Clear();
            foreach (var a in appointments.Where(a => a.AppointmentDate.Date < today))
                PastAppointments.Add(a);
        }

        public void CompleteAppointment(Appointment appointment)
        {
            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE Appointments SET Status = @status, Notes = @notes WHERE AppointmentID = @id", conn);
            cmd.Parameters.AddWithValue("@status", appointment.Status);
            cmd.Parameters.AddWithValue("@notes", appointment.Notes);
            cmd.Parameters.AddWithValue("@id", appointment.AppointmentID);
            cmd.ExecuteNonQuery();

            LoadAppointmentsFromDatabase();
        }
    }
}
