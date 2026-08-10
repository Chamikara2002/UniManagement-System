using System.Threading.Tasks;
using System.Web.Mvc;
using UniManage.Helpers;
using UniManage.Services;
using UniManage.ViewModels;

namespace UniManage.Controllers
{
    [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Lecturer)]
    public class ReportController : Controller
    {
        private readonly ReportService _reportService;

        public ReportController()
        {
            _reportService = new ReportService();
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> CoursePopularity(ReportFilterViewModel filter)
        {
            var data = await _reportService.GetCoursePopularityAsync(filter ?? new ReportFilterViewModel());
            return View(data);
        }

        public async Task<ActionResult> StudentPerformance(ReportFilterViewModel filter)
        {
            var data = await _reportService.GetStudentPerformanceAsync(filter ?? new ReportFilterViewModel());
            return View(data);
        }

        public async Task<ActionResult> WorkloadAnalysis(ReportFilterViewModel filter)
        {
            var data = await _reportService.GetWorkloadAnalysisAsync(filter ?? new ReportFilterViewModel());
            return View(data);
        }

        public async Task<ActionResult> EnrollmentSummary(ReportFilterViewModel filter)
        {
            var data = await _reportService.GetEnrollmentSummaryAsync(filter ?? new ReportFilterViewModel());
            return View(data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _reportService?.Dispose();
            base.Dispose(disposing);
        }
    }
}