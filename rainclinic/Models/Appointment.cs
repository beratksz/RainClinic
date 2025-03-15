using System;
using Microsoft.AspNetCore.Identity;

namespace rainclinic.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // IdentityUser ile ilişkilendirme
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Doctor { get; set; }
        public string MuayeneTipi { get; set; } // "Yüz yüze" veya "Online"
        public string AppointmentStatus { get; set; }  // Örneğin, "Bekliyor", "İşleme Alındı"
        public DateTime CreatedAt { get; set; }

        public IdentityUser User { get; set; }
    }
}
