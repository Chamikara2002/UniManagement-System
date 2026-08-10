using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Helpers;
using UniManage.Services;
using UniManage.ViewModels;

namespace UniManage.Controllers
{
    [Authorize(Roles = RoleNames.Lecturer)]
    public class LecturerController : Controller
    {
        private readonly LecturerService _lecturerService;

        public LecturerController()
        {
            _lecturerService = new LecturerService();
        }

        // GET: Lecturer/Index
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var vm = await _lecturerService.GetDashboardAsync(userId);
            if (vm == null) return HttpNotFound();
            return View("Dashboard", vm);
        }

        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var vm = await _lecturerService.GetDashboardAsync(userId);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        public async Task<ActionResult> MyCourses()
        {
            var userId = User.Identity.GetUserId();
            var courses = await _lecturerService.GetMyCoursesAsync(userId);
            return View(courses);
        }

        public async Task<ActionResult> CourseDetails(int id)
        {
            var course = await _lecturerService.GetCourseDetailsAsync(id);
            if (course == null) return HttpNotFound();
            return View(course);
        }

        [HttpGet]
        public ActionResult UploadMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadMaterial(int courseId, string title)
        {
            // file upload handling via Request.Files
            if (Request.Files.Count == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                ViewBag.CourseId = courseId;
                return View();
            }

            var file = Request.Files[0];
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("", "Invalid file.");
                ViewBag.CourseId = courseId;
                return View();
            }

            // Save file to ~/App_Data/Materials/{courseId}/
            var folder = Server.MapPath($"~/App_Data/Materials/{courseId}");
            System.IO.Directory.CreateDirectory(folder);
            var filePath = System.IO.Path.Combine(folder, System.IO.Path.GetFileName(file.FileName));
            file.SaveAs(filePath);

            var userId = User.Identity.GetUserId();
            await _lecturerService.UploadMaterialAsync(courseId, title ?? file.FileName, $"~/App_Data/Materials/{courseId}/{System.IO.Path.GetFileName(file.FileName)}", userId);

            return RedirectToAction("CourseDetails", new { id = courseId });
        }

        [HttpGet]
        public async Task<ActionResult> Assignments(int courseId)
        {
            var assigns = await _lecturerService.GetAssignmentsForCourseAsync(courseId);
            ViewBag.CourseId = courseId;
            return View(assigns);
        }

        [HttpGet]
        public ActionResult CreateAssignment(int courseId)
        {
            var vm = new CreateAssignmentViewModel { CourseId = courseId, DueDate = System.DateTime.UtcNow.AddDays(7) };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAssignment(CreateAssignmentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var userId = User.Identity.GetUserId();
            var lecturer = await _lecturerService.GetLecturerByUserIdAsync(userId);
            if (lecturer == null) return HttpNotFound();

            await _lecturerService.CreateAssignmentAsync(model, lecturer.Id);
            return RedirectToAction("Assignments", new { courseId = model.CourseId });
        }

        public async Task<ActionResult> Submissions(int assignmentId)
        {
            var subs = await _lecturerService.GetSubmissionsForAssignmentAsync(assignmentId);
            ViewBag.AssignmentId = assignmentId;
            return View(subs);
        }

        [HttpGet]
        public async Task<ActionResult> GradeSubmission(int submissionId)
        {
            var submission = await _lecturerService.GetSubmissionByIdAsync(submissionId);
            if (submission == null) return HttpNotFound();
            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GradeSubmission(int submissionId, decimal score, string feedback)
        {
            var userId = User.Identity.GetUserId();
            await _lecturerService.GradeSubmissionAsync(submissionId, score, feedback, userId);
            // Redirect back to submissions list; if assignmentId present in form, use it
            int assignmentId;
            if (int.TryParse(Request.Form["assignmentId"], out assignmentId))
            {
                return RedirectToAction("Submissions", new { assignmentId });
            }
            return RedirectToAction("Dashboard");
        }

        public async Task<ActionResult> Reports()
        {
            var userId = User.Identity.GetUserId();
            var lecturer = await _lecturerService.GetLecturerByUserIdAsync(userId);
            if (lecturer == null) return HttpNotFound();
            var report = await _lecturerService.GetReportsAsync(lecturer.Id);
            return View(report);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lecturerService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}