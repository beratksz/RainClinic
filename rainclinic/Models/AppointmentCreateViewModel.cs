using System.ComponentModel.DataAnnotations;

namespace rainclinic.Models
{
    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "İsim alanı zorunludur.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Doktor seçimi zorunludur.")]
        public string Doctor { get; set; }

        [Required(ErrorMessage = "Muayene tipi seçimi zorunludur.")]
        public string MuayeneTipi { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        public string Password { get; set; }
    }
}
