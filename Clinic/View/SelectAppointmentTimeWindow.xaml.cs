using System;
using System.Windows;
using Clinic.Models;

namespace Clinic.View
{
    public partial class SelectAppointmentTimeWindow : Window
    {
        public DateTime SelectedDateTime { get; private set; }

        public SelectAppointmentTimeWindow(Patient patient)
        {
            InitializeComponent();
            InitTimePickers();
            DatePicker.SelectedDate = DateTime.Today;

            if (patient != null)
            {
                PatientNameText.Text = $"{patient.LastName} {patient.FirstName}";
            }
        }

        private void InitTimePickers()
        {
            for (int hour = 8; hour <= 16; hour++)
                HourBox.Items.Add(hour.ToString("D2"));

            for (int min = 0; min < 60; min += 15)
                MinuteBox.Items.Add(min.ToString("D2"));

            HourBox.SelectedIndex = 0;
            MinuteBox.SelectedIndex = 0;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DatePicker.SelectedDate.HasValue || HourBox.SelectedItem == null || MinuteBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }

            var date = DatePicker.SelectedDate.Value;
            int hour = int.Parse(HourBox.SelectedItem.ToString());
            int minute = int.Parse(MinuteBox.SelectedItem.ToString());
            SelectedDateTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
            DialogResult = true;
            Close();
        }
    }
}