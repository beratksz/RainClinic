using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using rainclinic.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using rainclinic.Data;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments.ToListAsync();
            var model = appointments.Select(a => {
                var user = _userManager.FindByIdAsync(a.UserId).Result;
                return new AppointmentViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Email = a.Email,
                    Phone = a.Phone,
                    Doctor = a.Doctor,
                    MuayeneTipi = a.MuayeneTipi,
                    AppointmentStatus = a.AppointmentStatus,
                    CreatedAt = a.CreatedAt,
                    EmailConfirmed = user?.EmailConfirmed ?? false
                };
            }).ToList();

            return View(model);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Appointment model)
        {
            ModelState.Remove(nameof(model.User));

            if (!ModelState.IsValid)
                return View(model);

            var appointment = _context.Appointments.Find(model.Id);
            if (appointment == null) return NotFound();

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

        [HttpGet]
        public IActionResult Details(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

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
