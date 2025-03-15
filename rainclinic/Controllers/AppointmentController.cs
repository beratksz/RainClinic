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

            // Kullanıcı oluşturma
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = false // Doğrulama gerekli
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Otomatik olarak "User" rolü atama
            await _userManager.AddToRoleAsync(user, "User");

            // Randevu kaydı oluşturma
            var appointment = new Appointment
            {
                UserId = user.Id,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Doctor = model.Doctor,
                MuayeneTipi = model.MuayeneTipi,
                AppointmentStatus = "Bekliyor",
                CreatedAt = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Email doğrulama işlemi için token oluştur ve callback URL oluştur
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = token }, protocol: Request.Scheme);

            // IEmailSender servisini kullanarak e-posta gönderimi yap
            await _emailSender.SendEmailAsync(model.Email, "E-posta Onayı",
                $"Lütfen e-posta adresinizi onaylamak için <a href='{callbackUrl}'>buraya tıklayın</a>.");

            TempData["SuccessMessage"] = "Randevunuz oluşturuldu. Lütfen e-postanızı doğrulayın.";
            return RedirectToAction("Track");
        }

        // GET: /Appointment/Track
        [HttpGet]
        public async Task<IActionResult> Track()
        {
            // Giriş yapmış kullanıcının randevu kaydını getirir
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Kullanıcının randevusunu getirirken, kullanıcı ile Appointment arasında 1:1 ilişki varsayılıyor.
            var appointment = await _context.Appointments.SingleOrDefaultAsync(a => a.UserId == user.Id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Henüz randevu oluşturulmamış.";
                return RedirectToAction("Create");
            }

            var model = new AppointmentTrackViewModel
            {
                Name = appointment.Name,
                Email = appointment.Email,
                Phone = appointment.Phone,
                Doctor = appointment.Doctor,
                MuayeneTipi = appointment.MuayeneTipi,
                AppointmentStatus = appointment.AppointmentStatus,
                CreatedAt = appointment.CreatedAt,
                EmailConfirmed = user.EmailConfirmed
            };
            return View(model);
        }
    }
}
