﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Clinic.Models;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Windows.Input;
using System.Windows;

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
        }

        private void RegisterPatient(object obj)
        {
            string fullName = obj as string;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Введіть повне ім’я пацієнта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = DateTime.Now.Date;
            AppointmentService.RegisterAppointment(fullName, _doctorId, date, "Doctor");
            LoadAppointmentsFromDatabase();
        }

        public void LoadAppointmentsFromDatabase()
        {
            var appointments = new List<Appointment>();

            using (var connection = Clinic.DB.ClinicDB.GetConnection())
            {
                connection.Open();

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

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctorId", _doctorId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var model = new Appointment
                            {
                                AppointmentID = reader.GetInt32("AppointmentID"),
                                PatientName = reader.GetString("PatientName"),
                                AppointmentDate = reader.GetDateTime("AppointmentDate"),
                                PatientID = reader.GetInt32("PatientID"),
                                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "Очікується" : reader.GetString("Status"),
                                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? string.Empty : reader.GetString("Notes")
                            };
                            appointments.Add(model);
                        }
                    }
                }
            }

            var today = DateTime.Today;

            TodayAppointments.Clear();
            foreach (var a in appointments
                .Where(a => a.AppointmentDate.Date == today)
                .OrderBy(a => a.Status == "Прийом завершено")
                .ThenBy(a => a.AppointmentDate))
                TodayAppointments.Add(a);

            UpcomingAppointments.Clear();
            foreach (var a in appointments
                .Where(a => a.AppointmentDate.Date > today)
                .OrderBy(a => a.Status == "Прийом завершено")
                .ThenBy(a => a.AppointmentDate))
                UpcomingAppointments.Add(a);

            PastAppointments.Clear();
            foreach (var a in appointments
                .Where(a => a.AppointmentDate.Date < today)
                .OrderBy(a => a.Status == "Прийом завершено")
                .ThenByDescending(a => a.AppointmentDate))
                PastAppointments.Add(a);


            var firstToday = TodayAppointments.FirstOrDefault(a => a.Status != "Прийом завершено");
            if (firstToday != null)
            {
                firstToday.IsNearestToday = true;
            }
        }

        public void CompleteAppointment(Appointment appointment)
        {
            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();

            string query = @"
                            UPDATE Appointments
                            SET Status = @status, Notes = @notes
                            WHERE AppointmentID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@status", "Прийом завершено");
            cmd.Parameters.AddWithValue("@notes", appointment.Notes);
            cmd.Parameters.AddWithValue("@id", appointment.AppointmentID);

            cmd.ExecuteNonQuery();
            LoadAppointmentsFromDatabase();
        }
    }
}
