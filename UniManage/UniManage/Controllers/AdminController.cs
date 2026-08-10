using System.Threading.Tasks;
using System.Web.Mvc;
using UniManage.Helpers;
using UniManage.Models;
using UniManage.Services;

namespace UniManage.Controllers
{
    [Authorize(Roles = RoleNames.Administrator)]
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;

        public AdminController()
        {
            _adminService = new AdminService();
        }

        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var vm = await _adminService.GetDashboardAsync(userId);
            return View("Dashboard", vm);
        }

        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var vm = await _adminService.GetDashboardAsync(userId);
            return View(vm);
        }

        public async Task<ActionResult> ManageUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        // GET: Admin/EditUser/5
        public async Task<ActionResult> EditUser(string id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(ApplicationUser model)
        {
            if (!ModelState.IsValid) return View(model);
            // Update active flag and other editable fields
            var user = await _adminService.GetUserByIdAsync(model.Id);
            if (user == null) return HttpNotFound();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.IsActive = model.IsActive;
            // Persist
            await _adminService.UpdateUserAsync(user);
            return RedirectToAction("ManageUsers");
        }

        public async Task<ActionResult> ManageCourses()
        {
            var courses = await _adminService.GetAllCoursesAsync();
            return View(courses);
        }

        public ActionResult CreateCourse()
        {
            return View(new Course());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCourse(Course model)
        {
            if (!ModelState.IsValid) return View(model);
            await _adminService.CreateCourseAsync(model);
            return RedirectToAction("ManageCourses");
        }

        public async Task<ActionResult> EditCourse(int id)
        {
            var course = await _adminService.GetCourseByIdAsync(id);
            if (course == null) return HttpNotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCourse(Course model)
        {
            if (!ModelState.IsValid) return View(model);
            await _adminService.UpdateCourseAsync(model);
            return RedirectToAction("ManageCourses");
        }

        public async Task<ActionResult> DeleteCourse(int id)
        {
            var course = await _adminService.GetCourseByIdAsync(id);
            if (course == null) return HttpNotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCourseConfirmed(int id)
        {
            await _adminService.DeleteCourseAsync(id);
            return RedirectToAction("ManageCourses");
        }

        public async Task<ActionResult> Reports()
        {
            var userId = User.Identity.GetUserId();
            var vm = await _adminService.GetDashboardAsync(userId);
            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _adminService?.Dispose();
            base.Dispose(disposing);
        }
    }
}