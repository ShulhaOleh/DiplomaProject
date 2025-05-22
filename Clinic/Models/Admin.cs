using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Models
{
    public class Admin
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName => "Системний адміністратор";
    }
}
