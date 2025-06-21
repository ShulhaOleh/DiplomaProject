using Clinic.Models;
using Clinic.DB;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using Clinic.ViewModels;

namespace Clinic.View.Receptionist
{
    public partial class SelectDoctorWindow : Window
    {
        private List<Clinic.Models.Doctor> _doctors;
        private List<ScheduleRow> _scheduleRows;
        private DateTime _selectedDate;

        public Clinic.Models.Doctor SelectedDoctor { get; private set; }
        public DateTime SelectedDateTime => _selectedDate.Date + SelectedTime;
        public TimeSpan SelectedTime { get; private set; }
        public string Note => NoteBox.Text;

        private Patient _patient;

        private readonly RegisterPatientViewModel _viewModel;

        public SelectDoctorWindow(Patient patient, RegisterPatientViewModel viewModel)
        {
            InitializeComponent();
            _patient = patient;
            _viewModel = viewModel;
            LoadSpecialties();
            PatientInfoText.Text = _patient != null
                ? $"{_patient.FullName}"
                : "Пацієнта не передано";
        }

        private void LoadSpecialties()
        {
            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Specialties", conn);
            using var reader = cmd.ExecuteReader();

            var specialties = new List<dynamic>();
            while (reader.Read())
            {
                specialties.Add(new
                {
                    SpecialtyID = reader.GetInt32("SpecialtyID"),
                    Name = reader.GetString("Name")
                });
            }

            SpecialtyComboBox.ItemsSource = specialties;
        }

        private void LoadSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (DatePicker.SelectedDate == null || SpecialtyComboBox.SelectedValue == null)
                return;

            _selectedDate = DatePicker.SelectedDate.Value;
            int specialtyID = (int)SpecialtyComboBox.SelectedValue;

            LoadDoctors(specialtyID);
            BuildScheduleTable();
        }

        private void LoadDoctors(int specialtyID)
        {
            _doctors = new List<Clinic.Models.Doctor>();

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                @"SELECT DoctorID, FirstName, LastName, FathersName,
                         DateOfBirth, PhoneNumber, BreakHour, SpecialtyID
                  FROM Doctors
                  WHERE SpecialtyID = @sid", conn);
            cmd.Parameters.AddWithValue("@sid", specialtyID);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _doctors.Add(new Clinic.Models.Doctor
                {
                    DoctorID = reader.GetInt32("DoctorID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    FathersName = reader["FathersName"]?.ToString(),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    PhoneNumber = reader["PhoneNumber"]?.ToString(),
                    BreakHour = reader.GetTimeSpan("BreakHour"),
                    SpecialtyID = reader.GetInt32("SpecialtyID")
                });
            }
        }

        private void BuildScheduleTable()
        {
            _scheduleRows = new List<ScheduleRow>();

            TimeSpan start = new(9, 0, 0);
            TimeSpan end = new(18, 0, 0);

            var allAppointments = LoadAppointmentsForDate();

            for (TimeSpan time = start; time < end; time += TimeSpan.FromHours(1))
            {
                var row = new ScheduleRow
                {
                    TimeSlot = time.ToString(@"hh\:mm"),
                };

                foreach (var doctor in _doctors)
                {
                    var isBusy = allAppointments
                        .Any(a => a.DoctorID == doctor.DoctorID && a.Time == time);

                    var isBreak = time.Hours == doctor.BreakHour.Hours;

                    row.CellsByDoctorID[doctor.DoctorID] = new ScheduleCell
                    {
                        IsBusy = isBusy,
                        IsBreak = isBreak,
                        IsFree = !isBusy && !isBreak
                    };
                }

                _scheduleRows.Add(row);
            }

            GenerateGridColumns();
        }

        private List<(int DoctorID, TimeSpan Time)> LoadAppointmentsForDate()
        {
            var list = new List<(int, TimeSpan)>();

            using var conn = ClinicDB.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                @"SELECT DoctorID, AppointmentDate FROM Appointments WHERE DATE(AppointmentDate) = @d", conn);
            cmd.Parameters.AddWithValue("@d", _selectedDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((
                    reader.GetInt32("DoctorID"),
                    reader.GetDateTime("AppointmentDate").TimeOfDay
                ));
            }

            return list;
        }

        private void GenerateGridColumns()
        {
            ScheduleGrid.Columns.Clear();

            ScheduleGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Час",
                Binding = new System.Windows.Data.Binding("TimeSlot"),
                IsReadOnly = true
            });

            foreach (var doctor in _doctors)
            {
                var templateColumn = new DataGridTemplateColumn
                {
                    Header = $"{doctor.LastName} {doctor.FirstName}"
                };

                var cellTemplate = new DataTemplate();

                var factory = new FrameworkElementFactory(typeof(Border));
                factory.SetBinding(Border.BackgroundProperty, new Binding($"CellsByDoctorID[{doctor.DoctorID}].CellColor"));
                factory.SetValue(Border.PaddingProperty, new Thickness(5));
                factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(0));

                var text = new FrameworkElementFactory(typeof(TextBlock));
                text.SetBinding(TextBlock.TextProperty, new Binding($"CellsByDoctorID[{doctor.DoctorID}].Display"));
                text.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                text.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);

                factory.AppendChild(text);
                cellTemplate.VisualTree = factory;

                templateColumn.CellTemplate = cellTemplate;
                ScheduleGrid.Columns.Add(templateColumn);
            }

            ScheduleGrid.ItemsSource = _scheduleRows;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (_patient == null)
            {
                MessageBox.Show("Пацієнта не передано.");
                return;
            }

            if (SelectedDoctor == null || SelectedTime == default)
            {
                MessageBox.Show("Будь ласка, оберіть вільний час прийому.");
                return;
            }

            _viewModel.Note = NoteBox.Text;
            var dateTime = SelectedDateTime;

            if (_viewModel.Role == "Receptionist")
                _viewModel.RegisterAppointment(_patient, dateTime, SelectedDoctor);
            else
                _viewModel.RegisterAppointment(_patient, dateTime);

            MessageBox.Show($"Пацієнта {_patient.FullName} успішно записано на {dateTime:dd.MM.yyyy HH:mm} до {SelectedDoctor.FullName}.");

            DialogResult = true;
            Close();
        }



        private void ScheduleGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (ScheduleGrid.SelectedItem is not ScheduleRow row || ScheduleGrid.CurrentColumn == null)
                return;

            int columnIndex = ScheduleGrid.Columns.IndexOf(ScheduleGrid.CurrentColumn);

            if (columnIndex <= 0 || columnIndex > _doctors.Count)
                return;

            var doctor = _doctors[columnIndex - 1];

            if (!row.CellsByDoctorID.TryGetValue(doctor.DoctorID, out var cellInfo))
                return;

            if (!cellInfo.IsFree)
            {
                MessageBox.Show("Цей час зайнятий або є перерва.");
                return;
            }

            SelectedDoctor = doctor;
            SelectedTime = TimeSpan.Parse(row.TimeSlot);

            MessageBox.Show($"Обрано лікаря: {doctor.FullName}, час: {SelectedTime:hh\\:mm}");
        }

        private void ScheduleGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dep = (DependencyObject)e.OriginalSource;

            while (dep != null && dep is not DataGridCell)
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is not DataGridCell cell)
                return;

            if (cell.Column.DisplayIndex == 0)
                return; // перша колонка – час

            if (cell.DataContext is not ScheduleRow row)
                return;

            int columnIndex = cell.Column.DisplayIndex;

            if (columnIndex <= 0 || columnIndex > _doctors.Count)
                return;

            var doctor = _doctors[columnIndex - 1];

            if (!row.CellsByDoctorID.TryGetValue(doctor.DoctorID, out var cellInfo))
                return;

            if (!cellInfo.IsFree)
            {
                MessageBox.Show("Цей час зайнятий або є перерва.");
                return;
            }

            SelectedDoctor = doctor;
            SelectedTime = TimeSpan.Parse(row.TimeSlot);

            MessageBox.Show($"Обрано лікаря: {doctor.FullName}, час: {SelectedTime:hh\\:mm}");
        }



    }


}
