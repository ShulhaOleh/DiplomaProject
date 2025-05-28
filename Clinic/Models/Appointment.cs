using System;

public class AppointmentModel
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
}
