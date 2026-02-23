using System.Windows;

namespace Clinic.Models
{
    public class Admin
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName => (string)Application.Current.FindResource("Sys_Admin");
    }
}
