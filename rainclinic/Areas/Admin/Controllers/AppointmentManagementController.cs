using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using rainclinic.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using rainclinic.Data;

namespace rainclinic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AppointmentManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentManagementController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/AppointmentManagement
        public IActionResult Index()
        {
            var appointments = _context.Appointments.ToList();
            // Admin, ayrıca kullanıcının EmailConfirmed durumunu da görmek isteyebilir
            foreach (var appointment in appointments)
            {
                var user = _userManager.FindByIdAsync(appointment.UserId).Result;
                appointment.AppointmentStatus = appointment.AppointmentStatus; // Zaten saklanıyor; dilerseniz EmailConfirmed bilgisini ayrı ekleyin
            }
            return View(appointments);
        }

        // GET: /Admin/AppointmentManagement/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // POST: /Admin/AppointmentManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Appointment model)
        {
            ModelState.Remove(nameof(model.User));

            if (!ModelState.IsValid)
                return View(model);

            var appointment = _context.Appointments.Find(model.Id);
            if (appointment == null) return NotFound();

            // Admin, randevu detaylarını düzenleyebilir
            appointment.Name = model.Name;
            appointment.Phone = model.Phone;
            appointment.Doctor = model.Doctor;
            appointment.MuayeneTipi = model.MuayeneTipi;
            appointment.AppointmentStatus = model.AppointmentStatus;
            _context.Appointments.Update(appointment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Randevu başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        // GET: /Admin/AppointmentManagement/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // POST: /Admin/AppointmentManagement/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();
            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Randevu başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}
