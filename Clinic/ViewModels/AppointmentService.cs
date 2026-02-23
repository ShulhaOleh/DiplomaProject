using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Clinic.ViewModels
{
    public static class AppointmentService
    {
        public static event EventHandler AppointmentAdded;
        public static void NotifyAppointmentAdded() => AppointmentAdded?.Invoke(null, EventArgs.Empty);

        public static void RegisterAppointment(string patientFullName, int doctorId, DateTime appointmentDate, string role)
        {
            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();

            int patientId;
            var findPatientCmd = new MySqlCommand("SELECT PatientID FROM Patients WHERE CONCAT(FirstName, ' ', LastName) = @name", conn);
            findPatientCmd.Parameters.AddWithValue("@name", patientFullName);
            var result = findPatientCmd.ExecuteScalar();

            if (result != null)
            {
                patientId = Convert.ToInt32(result);
            }
            else
            {
                if (role == "Doctor")
                {
                    MessageBox.Show(
                        (string)Application.Current.FindResource("Msg_PatientNotFoundDoctor"),
                        (string)Application.Current.FindResource("Title_Error"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nameParts = patientFullName.Split(" ");
                if (nameParts.Length < 2)
                {
                    MessageBox.Show(
                        (string)Application.Current.FindResource("Msg_EnterPatientName"),
                        (string)Application.Current.FindResource("Title_Error"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var insertPatientCmd = new MySqlCommand(
                    "INSERT INTO Patients (FirstName, LastName) VALUES (@fn, @ln); SELECT LAST_INSERT_ID();", conn);
                insertPatientCmd.Parameters.AddWithValue("@fn", nameParts[0]);
                insertPatientCmd.Parameters.AddWithValue("@ln", nameParts[1]);
                patientId = Convert.ToInt32(insertPatientCmd.ExecuteScalar());

                var insertCardCmd = new MySqlCommand(
                    "INSERT INTO AmbulatoryCards (PatientID) VALUES (@pid)", conn);
                insertCardCmd.Parameters.AddWithValue("@pid", patientId);
                insertCardCmd.ExecuteNonQuery();
            }

            var insertAppointment = new MySqlCommand(
                "INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, Status, Notes) VALUES (@pid, @docId, @date, @status, '')", conn);
            insertAppointment.Parameters.AddWithValue("@pid", patientId);
            insertAppointment.Parameters.AddWithValue("@docId", doctorId);
            insertAppointment.Parameters.AddWithValue("@date", appointmentDate);
            insertAppointment.Parameters.AddWithValue("@status", Clinic.Models.AppointmentStatuses.Expected);
            insertAppointment.ExecuteNonQuery();

            NotifyAppointmentAdded();

            MessageBox.Show(
                (string)Application.Current.FindResource("Msg_AppointmentRegistered"),
                (string)Application.Current.FindResource("Title_Done"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
