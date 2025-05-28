using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Clinic.Models;
using MySql.Data.MySqlClient;

namespace Clinic.ViewModels
{
    public class RegisterPatientViewModel : BaseViewModel
    {
        public ObservableCollection<Patient> FilteredPatients { get; set; } = new();
        private string _searchQuery;

        private readonly int _doctorId;

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

        public RegisterPatientViewModel(int doctorId)
        {
            _doctorId = doctorId;
        }

        private void LoadFilteredPatients(string query)
        {
            FilteredPatients.Clear();
            if (string.IsNullOrWhiteSpace(query)) return;

            using var conn = DB.ClinicDB.GetConnection();
            conn.Open();

            string sql = @"
                SELECT PatientID, FirstName, LastName, PhoneNumber, BirthDate
                FROM Patients
                WHERE CONCAT(FirstName, ' ', LastName) LIKE @q";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", $"%{query}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                FilteredPatients.Add(new Patient
                {
                    PatientID = reader.GetInt32("PatientID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    PhoneNumber = reader.GetString("PhoneNumber"),
                    BirthDate = reader.GetDateTime("BirthDate")
                });
            }
        }

        public void RegisterAppointment(Patient patient)
        {
            using var conn = DB.ClinicDB.GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO Appointments (DoctorID, PatientID, AppointmentDate, Status)
                VALUES (@doc, @pat, @date, 'Очікується')";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@doc", _doctorId);
            cmd.Parameters.AddWithValue("@pat", patient.PatientID);
            cmd.Parameters.AddWithValue("@date", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
