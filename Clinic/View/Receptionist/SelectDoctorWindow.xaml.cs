using Clinic.Models;
using Clinic.DB;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;

namespace Clinic.View.Receptionist
{
    public partial class SelectDoctorWindow : Window
    {
        public Clinic.Models.Doctor SelectedDoctor => (Clinic.Models.Doctor)DoctorGrid.SelectedItem;


        public SelectDoctorWindow()
        {
            InitializeComponent();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            var doctors = new ObservableCollection<Clinic.Models.Doctor>();

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT DoctorID, FirstName, LastName, Specialty FROM Doctors", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                doctors.Add(new Clinic.Models.Doctor
                {
                    DoctorID = reader.GetInt32("DoctorID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Specialty = reader.GetString("Specialty")
                });
            }

            DoctorGrid.ItemsSource = doctors;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (DoctorGrid.SelectedItem is Clinic.Models.Doctor)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть лікаря.");
            }
        }
    }
}
