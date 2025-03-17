using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using rainclinic.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace rainclinic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Listeleme
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var model = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                Roles = _userManager.GetRolesAsync(u).Result,
                CreatedAt = DateTime.Now 
            });
            return View(model);
        }

        // Kullanıcı ekleme: GET
        [HttpGet]
        public IActionResult Create()
        {
            var model = new EditUserViewModel
            {
                Roles = _roleManager.Roles.Select(r => r.Name).ToList(),
                SelectedRoles = new List<string>()
            };
            return View(model);
        }

        // Kullanıcı ekleme: POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EditUserViewModel model, string[] SelectedRoles, string Password)
        {
            ModelState.Remove(nameof(model.Id));

            if (model.SelectedRoles == null)
            {
                model.SelectedRoles = new List<string>();
            }

            foreach (var key in ModelState.Keys)
            {
                foreach (var error in ModelState[key].Errors)
                {
                    Console.WriteLine($"Key: {key} Error: {error.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            try
            {
                Console.WriteLine("User oluşturuluyor: " + model.Email);
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };
                Console.WriteLine("User oluşturuldu.");

                var result = await _userManager.CreateAsync(user, Password);

                if (result == null)
                {
                    Console.WriteLine("CreateAsync sonucu null.");
                }
                else if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine("Hata: " + error.Description);
                    }
                }

                if (result != null && result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(user, SelectedRoles);
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }

                if (result != null)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine("ModelState Hata: " + error.Description);
                        ModelState.AddModelError("", error.Description);
                    }
                }
                model.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
                model.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            Console.WriteLine("Edit GET action başlatıldı. id: " + id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                Console.WriteLine("User bulunamadı. id: " + id);
                return NotFound();
            }
            Console.WriteLine("User bulundu: " + user.Email);

            var userRoles = await _userManager.GetRolesAsync(user);
            Console.WriteLine("User rolleri alındı: " + string.Join(", ", userRoles));

            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            Console.WriteLine("Tüm roller alındı: " + string.Join(", ", allRoles));

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email, 
                Roles = allRoles,
                SelectedRoles = userRoles
            };
            Console.WriteLine("Model oluşturuldu. Email: " + model.Email);

            ModelState.Clear();
            Console.WriteLine("ModelState temizlendi.");

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model, string[] SelectedRoles)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("", "Email alanı boş olamaz.");
                return View(model);
            }

            user.Email = model.Email;
            user.UserName = model.Email; 

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRolesAsync(user, SelectedRoles);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Roles = roles,
                CreatedAt = DateTime.Now 
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Roles = roles,
                CreatedAt = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu.";
                return RedirectToAction("Index", "UserManagement", new { area = "Admin" });
            }
            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction("Index", "UserManagement", new { area = "Admin" });
        }


    }
}
