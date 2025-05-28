using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Clinic.Models;
using MySql.Data.MySqlClient;

namespace Clinic.View.Doctor
{
    public partial class CompleteAppointmentWindow : Window
    {
        private readonly AppointmentModel _appointment;
        private int _cardId;

        public CompleteAppointmentWindow(AppointmentModel appointment)
        {
            InitializeComponent();
            _appointment = appointment;

            LoadPatientInfo();
            LoadMedicalData();

            NotesBox.Text = appointment.Notes;
            DisableEditing();
        }

        private void LoadPatientInfo()
        {
            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();

            string query = @"
                SELECT ac.CardNumber, ac.BloodType, ac.AmbulatoryCardID,
                       CONCAT(p.FirstName, ' ', p.LastName) AS FullName
                FROM AmbulatoryCards ac
                JOIN Patients p ON p.PatientID = ac.PatientID
                WHERE p.PatientID = @pid";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", _appointment.PatientID);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CardNumberText.Text = reader.GetString("CardNumber");
                PatientNameText.Text = reader.GetString("FullName");
                _appointment.BloodType = reader.IsDBNull(reader.GetOrdinal("BloodType")) ? "" : reader.GetString("BloodType");
                _cardId = reader.GetInt32("AmbulatoryCardID");

                Debug.WriteLine("[INFO] Card: " + CardNumberText.Text);
                Debug.WriteLine("[INFO] Patient: " + PatientNameText.Text);
                Debug.WriteLine("[INFO] Blood: " + _appointment.BloodType);
            }

            BloodTypeComboBox.SelectedItem = BloodTypeComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == _appointment.BloodType);
        }

        private void LoadMedicalData()
        {
            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();

            Debug.WriteLine($"[INFO] Loading allergies for cardId = {_cardId}");
            var cmdA = new MySqlCommand("SELECT a.Name FROM Allergies a JOIN CardAllergies ca ON ca.AllergyID = a.AllergyID WHERE ca.AmbulatoryCardID = @cardId", conn);
            cmdA.Parameters.AddWithValue("@cardId", _cardId);
            using var readerA = cmdA.ExecuteReader();
            bool hasAllergy = false;
            while (readerA.Read())
            {
                var name = readerA.GetString(0);
                AllergyList.Items.Add(name);
                Debug.WriteLine("  Allergy: " + name);
                hasAllergy = true;
            }
            readerA.Close();
            if (!hasAllergy)
            {
                AllergyList.Items.Add("(немає записів)");
            }

            Debug.WriteLine($"[INFO] Loading diseases for cardId = {_cardId}");
            var cmdD = new MySqlCommand("SELECT d.Name FROM ChronicDiseases d JOIN CardChronicDiseases cd ON cd.DiseaseID = d.DiseaseID WHERE cd.AmbulatoryCardID = @cardId", conn);
            cmdD.Parameters.AddWithValue("@cardId", _cardId);
            using var readerD = cmdD.ExecuteReader();
            bool hasDisease = false;
            while (readerD.Read())
            {
                var name = readerD.GetString(0);
                DiseaseList.Items.Add(name);
                Debug.WriteLine("  Disease: " + name);
                hasDisease = true;
            }
            if (!hasDisease)
            {
                DiseaseList.Items.Add("(немає записів)");
            }
        }

        private void DisableEditing()
        {
            NotesBox.IsEnabled = false;
            BloodTypeComboBox.IsEnabled = false;
            AllergyList.IsEnabled = false;
            NewAllergyBox.IsEnabled = false;
            DiseaseList.IsEnabled = false;
            NewDiseaseBox.IsEnabled = false;
            AddAllergyButton.IsEnabled = false;
            RemoveAllergyButton.IsEnabled = false;
            AddDiseaseButton.IsEnabled = false;
            RemoveDiseaseButton.IsEnabled = false;
        }

        private void EnableEditing()
        {
            NotesBox.IsEnabled = true;
            BloodTypeComboBox.IsEnabled = true;
            AllergyList.IsEnabled = true;
            NewAllergyBox.IsEnabled = true;
            DiseaseList.IsEnabled = true;
            NewDiseaseBox.IsEnabled = true;
            AddAllergyButton.IsEnabled = true;
            RemoveAllergyButton.IsEnabled = true;
            AddDiseaseButton.IsEnabled = true;
            RemoveDiseaseButton.IsEnabled = true;
        }

        private void SaveBloodType()
        {
            var selected = (BloodTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            using var conn = Clinic.DB.ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("UPDATE AmbulatoryCards SET BloodType = @bt WHERE AmbulatoryCardID = @cardId", conn);
            cmd.Parameters.AddWithValue("@bt", selected);
            cmd.Parameters.AddWithValue("@cardId", _cardId);
            cmd.ExecuteNonQuery();
        }

        private void AddAllergy_Click(object sender, RoutedEventArgs e) { }
        private void RemoveAllergy_Click(object sender, RoutedEventArgs e) { }
        private void AddDisease_Click(object sender, RoutedEventArgs e) { }
        private void RemoveDisease_Click(object sender, RoutedEventArgs e) { }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            _appointment.Notes = NotesBox.Text;
            SaveBloodType();
            DialogResult = true;
            Close();
        }

        private void NoShow_Click(object sender, RoutedEventArgs e)
        {
            DisableEditing();
            _appointment.Status = "Пацієнт не з’явився";
        }

        private void EnableEditing_Click(object sender, RoutedEventArgs e)
        {
            EnableEditing();
            _appointment.Status = "Прийом завершено";
        }
    }

    public static class CommandExtensions
    {
        public static MySqlCommand With(this MySqlCommand cmd, Action<MySqlCommand> configure)
        {
            configure(cmd);
            return cmd;
        }
    }
}
