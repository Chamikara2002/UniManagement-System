using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Helpers;
using UniManage.Services;

namespace UniManage.Controllers
{
    [Authorize(Roles = RoleNames.Student)]
    public class EnrollmentController : Controller
    {
        private readonly EnrollmentService _enrollmentService;

        public EnrollmentController()
        {
            _enrollmentService = new EnrollmentService();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enroll(int courseId)
        {
            var userId = User.Identity.GetUserId();
            var result = await _enrollmentService.EnrollStudentAsync(userId, courseId);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction("BrowseCourses", "Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Drop(int courseId)
        {
            var userId = User.Identity.GetUserId();
            var result = await _enrollmentService.DropEnrollmentAsync(userId, courseId);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Enrollment dropped.";
            }
            return RedirectToAction("MyCourses", "Student");
        }

        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var list = await _enrollmentService.GetMyEnrollmentsAsync(userId);
            return View("MyEnrollments", list);
        }

        public async Task<ActionResult> MyEnrollments()
        {
            var userId = User.Identity.GetUserId();
            var list = await _enrollmentService.GetMyEnrollmentsAsync(userId);
            return View(list);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _enrollmentService?.Dispose();
            base.Dispose(disposing);
        }
    }
}