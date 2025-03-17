using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rainclinic.Models; 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace rainclinic.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager,
                                 UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var principal = await _signInManager.CreateUserPrincipalAsync(user);
                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home", new { area = "Admin" }) });
                    }
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                }
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            return Json(new { success = false });
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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


    }
}
