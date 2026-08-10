using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Helpers;
using UniManage.Services;
using UniManage.ViewModels;

namespace UniManage.Controllers
{
    [Authorize(Roles = RoleNames.Student)]
    public class StudentController : Controller
    {
        private readonly StudentService _studentService;

        public StudentController()
        {
            _studentService = new StudentService();
        }

        // GET: Student/Index
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var vm = await _studentService.GetDashboardAsync(userId);
            if (vm == null) return HttpNotFound();
            return View("Dashboard", vm); // Updated view return
        }

        // GET: Student/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var vm = await _studentService.GetDashboardAsync(userId);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        // GET: Student/BrowseCourses
        public async Task<ActionResult> BrowseCourses()
        {
            var userId = User.Identity.GetUserId();
            var courses = await _studentService.BrowseCoursesAsync(userId);
            return View(courses);
        }

        // GET: Student/CourseDetails/5
        public async Task<ActionResult> CourseDetails(int id)
        {
            var userId = User.Identity.GetUserId();
            var course = await _studentService.GetCourseDetailsAsync(id, userId);
            if (course == null) return HttpNotFound();
            return View(course);
        }

        // GET: Student/MyCourses
        public async Task<ActionResult> MyCourses()
        {
            var userId = User.Identity.GetUserId();
            var courses = await _studentService.GetMyCoursesAsync(userId);
            return View(courses);
        }

        // GET: Student/MyAssignments
        public async Task<ActionResult> MyAssignments()
        {
            var userId = User.Identity.GetUserId();
            var assignments = await _studentService.GetMyAssignmentsAsync(userId);
            return View(assignments);
        }

        // GET: Student/Messages - integration entry point
        public ActionResult Messages()
        {
            // Simple integration point: redirect to central MessageController inbox if present
            return RedirectToAction("Index", "Message");
        }

        // GET: Student/MyGrades
        public async Task<ActionResult> MyGrades()
        {
            var userId = User.Identity.GetUserId();
            var grades = await _studentService.GetMyGradesAsync(userId);
            return View(grades);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _studentService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
