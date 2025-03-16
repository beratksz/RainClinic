namespace rainclinic.Models
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Doctor { get; set; }
        public string MuayeneTipi { get; set; }
        public string AppointmentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
    }

}
