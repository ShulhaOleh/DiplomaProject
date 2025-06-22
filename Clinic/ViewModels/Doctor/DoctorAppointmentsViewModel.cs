using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Clinic.Models;
using MySql.Data.MySqlClient;

namespace Clinic.ViewModels.Doctor
{
    public class DoctorAppointmentsViewModel : BaseViewModel
    {
        public ObservableCollection<Appointment> TodayAppointments { get; set; } = new();
        public ObservableCollection<Appointment> UpcomingAppointments { get; set; } = new();
        public ObservableCollection<Appointment> PastAppointments { get; set; } = new();

        public ICommand RegisterPatientCommand { get; set; }

        private readonly int _doctorId;

        public DoctorAppointmentsViewModel(int doctorId)
        {
            _doctorId = doctorId;
            RegisterPatientCommand = new RelayCommand<object>(RegisterPatient);

            LoadAppointmentsFromDatabase();

            AppointmentService.AppointmentAdded += OnAppointmentAdded;
        }

        private void OnAppointmentAdded(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => LoadAppointmentsFromDatabase());
        }

        private void RegisterPatient(object obj)
        {
            if (obj is not string fullName || string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Введіть повне ім’я пацієнта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = DateTime.Now.Date;
            AppointmentService.RegisterAppointment(fullName, _doctorId, date, "Doctor");
        }

        public void LoadAppointmentsFromDatabase()
        {
            var appointments = new List<Appointment>();

            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();

            string query = @"
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

            using var cmd = new MySqlCommand(query, conn);
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
                    Status = reader.IsDBNull("Status") ? "Очікується" : reader.GetString("Status"),
                    Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes")
                });
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

            string query = "UPDATE Appointments SET Status = @status, Notes = @notes WHERE AppointmentID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@status", appointment.Status);
            cmd.Parameters.AddWithValue("@notes", appointment.Notes);
            cmd.Parameters.AddWithValue("@id", appointment.AppointmentID);
            cmd.ExecuteNonQuery();

            LoadAppointmentsFromDatabase();
        }


    }
}
