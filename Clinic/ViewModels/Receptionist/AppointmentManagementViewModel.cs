using Clinic.Models;
using Clinic.DB;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Clinic.ViewModels.Receptionist
{
    public class AppointmentManagementViewModel : BaseViewModel
    {
        public ObservableCollection<Appointment> TodayAppointments { get; set; } = new();
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

            string query = @"
                SELECT 
                    a.AppointmentID,
                    a.PatientID,
                    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
                    a.AppointmentDate,
                    a.Status,
                    a.Notes,
                    a.AmbulatoryCardID
                FROM Appointments a
                JOIN Patients p ON p.PatientID = a.PatientID
                WHERE DATE(a.AppointmentDate) = @today
                ORDER BY a.AppointmentDate ASC";

            using var cmd = new MySqlCommand(query, conn);
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
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "Очікується" : reader.GetString("Status"),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString("Notes"),
                    CardId = reader.GetInt32("AmbulatoryCardID")
                });
            }
        }

        private void MarkAsNoShow(Appointment appointment)
        {
            if (appointment == null || appointment.Status == "Прийом завершено" || appointment.Status == "Пацієнт не з’явився")
            {
                MessageBox.Show("Цей прийом уже завершено або позначено як 'не з’явився'.");
                return;
            }

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            string update = "UPDATE Appointments SET Status = 'Пацієнт не з’явився' WHERE AppointmentID = @id";
            using var cmd = new MySqlCommand(update, conn);
            cmd.Parameters.AddWithValue("@id", appointment.AppointmentID);
            cmd.ExecuteNonQuery();

            MessageBox.Show($"Прийом пацієнта '{appointment.PatientName}' позначено як 'не з’явився'.", "Готово");
            LoadTodayAppointments();
        }
    }
}
