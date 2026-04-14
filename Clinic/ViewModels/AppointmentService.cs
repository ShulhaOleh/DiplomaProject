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

            // Read phase — find patient before opening a transaction
            var findCmd = new MySqlCommand(
                "SELECT PatientID FROM Patients WHERE CONCAT(FirstName, ' ', LastName) = @name", conn);
            findCmd.Parameters.AddWithValue("@name", patientFullName);
            var existing = findCmd.ExecuteScalar();

            // Validate before any writes
            string[] nameParts = null;
            if (existing == null)
            {
                if (role == "Doctor")
                {
                    MessageBox.Show(
                        (string)Application.Current.FindResource("Msg_PatientNotFoundDoctor"),
                        (string)Application.Current.FindResource("Title_Error"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                nameParts = patientFullName.Split(" ");
                if (nameParts.Length < 2)
                {
                    MessageBox.Show(
                        (string)Application.Current.FindResource("Msg_EnterPatientName"),
                        (string)Application.Current.FindResource("Title_Error"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Write phase — all inserts in a single transaction
            using var transaction = conn.BeginTransaction();
            try
            {
                int patientId;

                if (existing != null)
                {
                    patientId = Convert.ToInt32(existing);
                }
                else
                {
                    var insertPatient = new MySqlCommand(
                        "INSERT INTO Patients (FirstName, LastName) VALUES (@fn, @ln); SELECT LAST_INSERT_ID();",
                        conn, transaction);
                    insertPatient.Parameters.AddWithValue("@fn", nameParts[0]);
                    insertPatient.Parameters.AddWithValue("@ln", nameParts[1]);
                    patientId = Convert.ToInt32(insertPatient.ExecuteScalar());

                    var insertCard = new MySqlCommand(
                        "INSERT INTO AmbulatoryCards (PatientID) VALUES (@pid)",
                        conn, transaction);
                    insertCard.Parameters.AddWithValue("@pid", patientId);
                    insertCard.ExecuteNonQuery();
                }

                var insertAppt = new MySqlCommand(
                    "INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, Status, Notes) VALUES (@pid, @docId, @date, @status, '')",
                    conn, transaction);
                insertAppt.Parameters.AddWithValue("@pid", patientId);
                insertAppt.Parameters.AddWithValue("@docId", doctorId);
                insertAppt.Parameters.AddWithValue("@date", appointmentDate);
                insertAppt.Parameters.AddWithValue("@status", Clinic.Models.AppointmentStatuses.Expected);
                insertAppt.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            NotifyAppointmentAdded();

            MessageBox.Show(
                (string)Application.Current.FindResource("Msg_AppointmentRegistered"),
                (string)Application.Current.FindResource("Title_Done"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
