using Clinic.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Clinic.ViewModels.Doctor
{
    public class DoctorAppointmentsViewModel : BaseViewModel
    {
        public ObservableCollection<AppointmentModel> Appointments { get; set; }

        private readonly int _doctorId;

        public DoctorAppointmentsViewModel(int doctorId)
        {
            _doctorId = doctorId;
            Appointments = new ObservableCollection<AppointmentModel>();
            LoadAppointmentsFromDatabase();
        }

        private void LoadAppointmentsFromDatabase()
        {
            using (var connection = Clinic.DB.ClinicDB.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
                           a.AppointmentDate,
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
                            var appointmentDate = reader.GetDateTime("AppointmentDate");

                            var model = new AppointmentModel
                            {
                                PatientName = reader.GetString("PatientName"),
                                Date = appointmentDate.Date,
                                Time = appointmentDate.ToString("HH:mm"),
                                Reason = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString("Notes")
                            };

                            Appointments.Add(model);
                        }
                    }
                }
            }
        }
    }

    public class AppointmentModel
    {
        public string PatientName { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; } // hh:mm
        public string Reason { get; set; }
    }
}
