using System;

public class AppointmentModel
{
    public string PatientName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Notes { get; set; }
    public string Status { get; set; }
    public bool IsNearestToday { get; set; } = false;
}
