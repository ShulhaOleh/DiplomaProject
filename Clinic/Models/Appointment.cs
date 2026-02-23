using System;
using System.Windows;

public class Appointment
{
    public int AppointmentID { get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Notes { get; set; }
    public int CardId { get; set; }
    public string Status { get; set; }
    public string BloodType { get; set; }
    // Highlight nearest today's appointment
    public bool IsNearestToday { get; set; } = false;

    public bool IsCompleted => Status == Clinic.Models.AppointmentStatuses.Completed;
    public bool IsNoShow    => Status == Clinic.Models.AppointmentStatuses.NoShow;

    public string StatusDisplay
    {
        get
        {
            if (Status == Clinic.Models.AppointmentStatuses.Completed)
                return (string)Application.Current.FindResource("Status_Completed");
            if (Status == Clinic.Models.AppointmentStatuses.NoShow)
                return (string)Application.Current.FindResource("Status_NoShow");
            return (string)Application.Current.FindResource("Status_Expected");
        }
    }
}
