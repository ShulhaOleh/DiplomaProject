using System;

namespace Clinic.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int? LinkedID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FathersName { get; set; }
        public string FullName => $"{LastName} {FirstName} {FathersName}";
        public string Phone { get; set; }
    }
}

