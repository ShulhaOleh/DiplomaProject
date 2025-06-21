using System;

namespace Clinic.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FathersName { get; set; }
        public int SpecialtyID { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public TimeSpan BreakHour { get; set; }

        public string FullName => $"{LastName} {FirstName} {FathersName}";
    }
}