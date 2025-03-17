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
        private readonly IEmailSender _emailSender;

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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var openAppointment = await _context.Appointments.AnyAsync(a => a.UserId == currentUser.Id && a.AppointmentStatus != "Kapandı");
                if (openAppointment)
                {
                    TempData["ErrorMessage"] = "Mevcut randevunuz kapatılmadan yeni randevu oluşturamazsınız.";
                    return RedirectToAction("Track");
                }
            }
            else
            {
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

            IdentityUser user;
            if (!User.Identity.IsAuthenticated)
            {
                user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = false 
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                user = await _userManager.GetUserAsync(User);
            }

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
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"ID '{userId}' ile bir kullanıcı bulunamadı.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return View("ConfirmEmail"); 
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Error"); 
            }
        }


        [HttpGet]
        public async Task<IActionResult> Track()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var appointments = await _context.Appointments
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

    }
}
