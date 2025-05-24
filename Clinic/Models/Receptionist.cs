namespace Clinic.Models
{
    public class Receptionist
    {
        public int AdministratorID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}