using Clinic.DB;
using Clinic.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Clinic.ViewModels
{
    public class RegisterPatientViewModel : BaseViewModel
    {
        private readonly int _userId;
        private readonly string _role;

        public ObservableCollection<Patient> FilteredPatients { get; } = new();
        private string _searchQuery;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    LoadFilteredPatients(_searchQuery);
                }
            }
        }

        private string _note;
        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged();
            }
        }

        public RegisterPatientViewModel(int id, string role = "Doctor")
        {
            _userId = id;
            _role = role;

            LoadFilteredPatients(string.Empty);
        }

        private void LoadFilteredPatients(string query)
        {
            FilteredPatients.Clear();

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            string sql = "SELECT PatientID, FirstName, LastName, PhoneNumber, DateOfBirth FROM Patients";
            var parts = query?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

            if (parts.Length == 1)
                sql += " WHERE FirstName LIKE @p1 OR LastName LIKE @p1";
            else if (parts.Length >= 2)
                sql += " WHERE (FirstName LIKE @p1 AND LastName LIKE @p2) OR (FirstName LIKE @p2 AND LastName LIKE @p1)";

            using var cmd = new MySqlCommand(sql, conn);
            if (parts.Length == 1)
                cmd.Parameters.AddWithValue("@p1", $"%{parts[0]}%");
            else if (parts.Length >= 2)
            {
                cmd.Parameters.AddWithValue("@p1", $"%{parts[0]}%");
                cmd.Parameters.AddWithValue("@p2", $"%{parts[1]}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                FilteredPatients.Add(new Patient
                {
                    PatientID = reader.GetInt32("PatientID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    PhoneNumber = reader.GetString("PhoneNumber"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth")
                });
            }
        }

        public void RegisterAppointment(Patient patient, DateTime selectedDateTime)
        {
            using var conn = ClinicDB.GetConnection();
            conn.Open();

            int patientId = patient.PatientID;

            int cardId;
            using (var cmdCard = new MySqlCommand("SELECT AmbulatoryCardID FROM AmbulatoryCards WHERE PatientID = @pid", conn))
            {
                cmdCard.Parameters.AddWithValue("@pid", patientId);
                var result = cmdCard.ExecuteScalar();

                if (result != null)
                {
                    cardId = Convert.ToInt32(result);
                }
                else
                {
                    var insertCardCmd = new MySqlCommand(
                        "INSERT INTO AmbulatoryCards (PatientID, CreationDate, CardNumber) VALUES (@pid, NOW(), UUID()); SELECT LAST_INSERT_ID();", conn);
                    insertCardCmd.Parameters.AddWithValue("@pid", patientId);
                    cardId = Convert.ToInt32(insertCardCmd.ExecuteScalar());
                }
            }

            var checkCmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM Appointments 
                WHERE PatientID = @pat AND DoctorID = @doc AND DATE(AppointmentDate) = @day", conn);
            checkCmd.Parameters.AddWithValue("@doc", _userId);
            checkCmd.Parameters.AddWithValue("@pat", patientId);
            checkCmd.Parameters.AddWithValue("@day", selectedDateTime.Date);

            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (exists > 0)
            {
                MessageBox.Show("Пацієнт уже записаний на цей день.");
                return;
            }

            string query;
            var cmd = new MySqlCommand();
            cmd.Connection = conn;

            if (_role == "Receptionist")
            {
                var doctorId = PromptDoctorId();
                if (doctorId == null)
                    return;

                query = @"
                    INSERT INTO Appointments 
                        (DoctorID, PatientID, AppointmentDate, Status, AmbulatoryCardID, ReceptionistID, Notes)
                    VALUES 
                        (@doc, @pat, @date, 'Очікується', @card, @rec, @note)";

                cmd.Parameters.AddWithValue("@doc", doctorId);
                cmd.Parameters.AddWithValue("@rec", _userId);
            }
            else
            {
                query = @"
                    INSERT INTO Appointments 
                        (DoctorID, PatientID, AppointmentDate, Status, AmbulatoryCardID, Notes)
                    VALUES 
                        (@doc, @pat, @date, 'Очікується', @card, @note)";

                cmd.Parameters.AddWithValue("@doc", _userId);
            }

            cmd.Parameters.AddWithValue("@pat", patientId);
            cmd.Parameters.AddWithValue("@date", selectedDateTime);
            cmd.Parameters.AddWithValue("@card", cardId);
            cmd.Parameters.AddWithValue("@note", string.IsNullOrWhiteSpace(Note) ? "" : Note);

            cmd.CommandText = query;
            cmd.ExecuteNonQuery();

            MessageBox.Show("Пацієнта успішно записано на прийом.");
        }

        private int? PromptDoctorId()
        {
            var window = new View.Receptionist.SelectDoctorWindow();
            if (window.ShowDialog() == true)
            {
                Note = window.Note;
                return window.SelectedDoctor.DoctorID;
            }
            return null;
        }

    }
}
