using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rainclinic.Models;
using System;
using System.Threading.Tasks;
using rainclinic.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace rainclinic.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender; // E-posta göndermek için

        public AppointmentController(UserManager<IdentityUser> userManager,
                                     RoleManager<IdentityRole> roleManager,
                                     ApplicationDbContext context,
                                     IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailSender = emailSender;
        }

        // GET: /Appointment/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        


        // POST: /Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Eğer kullanıcı oturum açmışsa
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                // Kullanıcının kapalı olmayan (open) bir randevusu var mı kontrol et
                var openAppointment = await _context.Appointments.AnyAsync(a => a.UserId == currentUser.Id && a.AppointmentStatus != "Kapandı");
                if (openAppointment)
                {
                    TempData["ErrorMessage"] = "Mevcut randevunuz kapatılmadan yeni randevu oluşturamazsınız.";
                    return RedirectToAction("Track");
                }
            }
            else
            {
                // Kullanıcı giriş yapmamışsa, aynı email ile kayıtlı kullanıcı var mı kontrol et
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    var openAppointment = await _context.Appointments.AnyAsync(a => a.UserId == existingUser.Id && a.AppointmentStatus != "Kapandı");
                    if (openAppointment)
                    {
                        TempData["ErrorMessage"] = "Zaten açık randevunuz var. Lütfen mevcut randevunuz kapandıktan sonra yeni randevu oluşturun.";
                        return RedirectToAction("Track");
                    }
                }
            }

            // Kullanıcı oluşturma (eğer henüz kayıtlı değilse)
            IdentityUser user;
            if (!User.Identity.IsAuthenticated)
            {
                user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = false // Email doğrulama gerekli
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }
                // Yeni kayıt olan kullanıcılara otomatik olarak "User" rolü atanır
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                user = await _userManager.GetUserAsync(User);
            }

            // Randevu kaydı oluşturma
            var appointment = new Appointment
            {
                UserId = user.Id,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Doctor = model.Doctor,
                MuayeneTipi = model.MuayeneTipi,
                AppointmentStatus = "Bekliyor", // Yeni randevu başlangıçta açık (bekleyen) statüsünde
                CreatedAt = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Email doğrulama işlemi için token oluşturma ve callback URL üretimi (Identity’nin kendi mekanizması)
            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = token }, protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "E-posta Onayı",
                    $"Lütfen e-posta adresinizi onaylamak için <a href='{callbackUrl}'>buraya tıklayın</a>.");
            }

            TempData["SuccessMessage"] = "Randevunuz oluşturuldu. Lütfen e-postanızı doğrulayın.";
            return RedirectToAction("Track");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                // Geçersiz parametreler; anasayfaya yönlendirme yapabilirsiniz.
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Kullanıcı bulunamazsa uygun bir hata mesajı döndürün.
                return NotFound($"ID '{userId}' ile bir kullanıcı bulunamadı.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                // Onay başarılıysa, kullanıcıya onaylandığına dair bir view veya mesaj gösterin.
                return View("ConfirmEmail"); // Örneğin, "Email onaylandı" mesajı içeren view
            }
            else
            {
                // Onay başarısızsa hata view'i veya uygun mesaj gösterin.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Error"); // Hata view'i
            }
        }


        // GET: /Appointment/Track
        [HttpGet]
        public async Task<IActionResult> Track()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Kullanıcının tüm randevularını getir
            var appointments = await _context.Appointments
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

    }
}
