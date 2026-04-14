using System;
using System.Windows;

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

        public string RoleDisplay
        {
            get
            {
                var key = Role switch
                {
                    "Doctor" => "Role_Doctor",
                    "Receptionist" => "Role_Receptionist",
                    "Admin" => "Role_Admin",
                    _ => null
                };
                if (key != null && Application.Current.FindResource(key) is string localized)
                    return localized;
                return Role;
            }
        }
    }
}
