using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Services;
using UniManage.ViewModels;

namespace UniManage.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly AssignmentService _assignmentService;

        public AssignmentController()
        {
            _assignmentService = new AssignmentService();
        }

        // GET: Assignment/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var assignment = await _assignmentService.GetAssignmentAsync(id);
            if (assignment == null) return HttpNotFound();

            var vm = new AssignmentDetailsViewModel
            {
                Assignment = assignment,
                IsEnrolled = false,
                StudentSubmission = null,
                CanSubmit = false
            };

            if (User.IsInRole("Student"))
            {
                var userId = User.Identity.GetUserId();
                var student = await new Services.EnrollmentService().GetStudentByUserIdAsync(userId);
                if (student != null)
                {
                    vm.IsEnrolled = await _assignmentService.CanStudentSubmitAsync(student.Id, id);
                    vm.StudentSubmission = await _assignmentService.GetStudentSubmissionAsync(id, student.Id);
                    vm.CanSubmit = vm.IsEnrolled && (vm.StudentSubmission == null || assignment.DueDate >= System.DateTime.UtcNow);
                }
            }

            return View(vm);
        }

        // GET: Assignment/Submit/5
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> Submit(int id)
        {
            var userId = User.Identity.GetUserId();
            var student = await new Services.EnrollmentService().GetStudentByUserIdAsync(userId);
            if (student == null) return HttpNotFound();

            var can = await _assignmentService.CanStudentSubmitAsync(student.Id, id);
            if (!can) return new HttpStatusCodeResult(403);

            var vm = new SubmitAssignmentViewModel { AssignmentId = id };
            return View(vm);
        }

        // POST: Assignment/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> Submit(SubmitAssignmentViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var student = await new Services.EnrollmentService().GetStudentByUserIdAsync(userId);
            if (student == null) return HttpNotFound();

            // file handling - save to App_Data/Assignments/{assignmentId}/
            string filePath = null;
            if (model.File != null && model.File.ContentLength > 0)
            {
                var folder = Server.MapPath($"~/App_Data/Assignments/{model.AssignmentId}");
                System.IO.Directory.CreateDirectory(folder);
                var fileName = System.IO.Path.GetFileName(model.File.FileName);
                var savePath = System.IO.Path.Combine(folder, fileName);
                model.File.SaveAs(savePath);
                filePath = $"~/App_Data/Assignments/{model.AssignmentId}/{fileName}";
            }

            var res = await _assignmentService.SubmitAssignmentAsync(student.Id, model, filePath);
            if (!res.Success)
            {
                TempData["Error"] = res.Message;
            }
            else
            {
                TempData["Success"] = res.Message;
            }

            return RedirectToAction("Details", new { id = model.AssignmentId });
        }

        // GET: Assignment/Submissions/5 (assignmentId)
        [Authorize(Roles = "Lecturer,Administrator")]
        public async Task<ActionResult> Submissions(int assignmentId)
        {
            var subs = await new Services.LecturerService().GetSubmissionsForAssignmentAsync(assignmentId);
            ViewBag.AssignmentId = assignmentId;
            return View("SubmissionsList", subs);
        }

        [Authorize(Roles = "Lecturer,Administrator")]
        public async Task<ActionResult> Grade(int submissionId)
        {
            var sub = await _assignmentService.GetSubmissionAsync(submissionId);
            if (sub == null) return HttpNotFound();
            return View("GradeSubmission", sub);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Administrator")]
        public async Task<ActionResult> Grade(int submissionId, decimal score, string feedback)
        {
            var userId = User.Identity.GetUserId();
            var (success, message) = await _assignmentService.GradeSubmissionAsync(submissionId, score, feedback, userId);
            if (!success) TempData["Error"] = message; else TempData["Success"] = message;
            var sub = await _assignmentService.GetSubmissionAsync(submissionId);
            if (sub == null) return RedirectToAction("Index", "Home");
            return RedirectToAction("Submissions", new { assignmentId = sub.AssignmentId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _assignmentService?.Dispose();
            base.Dispose(disposing);
        }
    }
}