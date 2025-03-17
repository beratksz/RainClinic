using System;
using Microsoft.AspNetCore.Identity;

namespace rainclinic.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string UserId { get; set; }  
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Doctor { get; set; }
        public string MuayeneTipi { get; set; }
        public string AppointmentStatus { get; set; }  
        public DateTime CreatedAt { get; set; }

        public IdentityUser User { get; set; }
    }
}
