using System.Threading.Tasks;
using System.Web.Mvc;
using UniManage.Helpers;
using UniManage.Services;
using UniManage.ViewModels;

namespace UniManage.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController()
        {
            _authService = new AuthService();
        }

        public ActionResult Login(string returnUrl)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl, null);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var result = await _authService.LoginAsync(model, HttpContext);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Unable to log in.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            return RedirectToLocal(returnUrl, result.PrimaryRole);
        }

        public ActionResult Register()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Role != RoleNames.Student &&
                model.Role != RoleNames.Lecturer &&
                model.Role != RoleNames.Administrator)
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
                return View(model);
            }

            var result = await _authService.RegisterAsync(model, HttpContext);
            if (!result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                }

                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _authService.Logout(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _authService?.Dispose();
            }

            base.Dispose(disposing);
        }

        private ActionResult RedirectToLocal(string returnUrl, string primaryRole)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            switch (primaryRole)
            {
                case RoleNames.Administrator:
                    return RedirectToAction("Dashboard", "Admin");
                case RoleNames.Lecturer:
                    return RedirectToAction("Dashboard", "Lecturer");
                case RoleNames.Student:
                    return RedirectToAction("Dashboard", "Student");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }
    }
}
