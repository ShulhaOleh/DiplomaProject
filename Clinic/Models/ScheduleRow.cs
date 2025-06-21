using System.Collections.Generic;

namespace Clinic.Models
{
    public class ScheduleRow
    {
        public string TimeSlot { get; set; } 
        public Dictionary<int, ScheduleCell> CellsByDoctorID { get; set; } = new();
    }
}
