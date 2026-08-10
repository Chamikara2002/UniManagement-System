using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UniManage.Data;
using UniManage.ViewModels;

namespace UniManage.Services
{
    public class ReportService : IDisposable
    {
        private readonly UniManageDbContext _context;

        public ReportService()
        {
            _context = new UniManageDbContext();
        }

        public async Task<List<CoursePopularityViewModel>> GetCoursePopularityAsync(ReportFilterViewModel filter)
        {
            var q = _context.Courses.AsQueryable();

            if (filter.CourseId.HasValue) q = q.Where(c => c.Id == filter.CourseId.Value);
            if (filter.LecturerId.HasValue) q = q.Where(c => c.LecturerId == filter.LecturerId.Value);

            var courses = await q.Include(c => c.Enrollments).ToListAsync();

            var results = courses.Select(c => new CoursePopularityViewModel
            {
                CourseId = c.Id,
                Code = c.Code,
                Title = c.Title,
                EnrolledCount = c.Enrollments?.Count(e => e.Status == Models.EnrollmentStatus.Enrolled &&
                    (!filter.From.HasValue || e.EnrolledOn >= filter.From.Value) &&
                    (!filter.To.HasValue || e.EnrolledOn <= filter.To.Value)) ?? 0
            }).OrderByDescending(r => r.EnrolledCount).ToList();

            return results;
        }

        public async Task<List<StudentPerformanceViewModel>> GetStudentPerformanceAsync(ReportFilterViewModel filter)
        {
            // Grades -> AssignmentSubmission -> Assignment -> Course
            var gradeQ = _context.Grades.Include(g => g.AssignmentSubmission.Assignment).Include(g => g.AssignmentSubmission.Student.User).AsQueryable();

            if (filter.CourseId.HasValue)
            {
                gradeQ = gradeQ.Where(g => g.AssignmentSubmission.Assignment.CourseId == filter.CourseId.Value);
            }
            if (filter.LecturerId.HasValue)
            {
                gradeQ = gradeQ.Where(g => g.AssignmentSubmission.Assignment.LecturerId == filter.LecturerId.Value);
            }
            if (filter.From.HasValue)
            {
                gradeQ = gradeQ.Where(g => g.GradedOn >= filter.From.Value);
            }
            if (filter.To.HasValue)
            {
                gradeQ = gradeQ.Where(g => g.GradedOn <= filter.To.Value);
            }

            var grades = await gradeQ.ToListAsync();

            var grouped = grades.GroupBy(g => new { g.AssignmentSubmission.StudentId, g.AssignmentSubmission.Student.User.UserName, g.AssignmentSubmission.Student.User.FirstName, g.AssignmentSubmission.Student.User.LastName })
                .Select(g => new StudentPerformanceViewModel
                {
                    StudentId = g.Key.StudentId,
                    StudentUserName = g.Key.UserName,
                    StudentName = string.IsNullOrEmpty(g.Key.FirstName) && string.IsNullOrEmpty(g.Key.LastName) ? g.Key.UserName : g.Key.FirstName + " " + g.Key.LastName,
                    AverageScore = g.Average(x => (double)x.Score)
                }).OrderByDescending(x => x.AverageScore).ToList();

            return grouped;
        }

        public async Task<List<WorkloadAnalysisViewModel>> GetWorkloadAnalysisAsync(ReportFilterViewModel filter)
        {
            var lecturers = _context.Lecturers.Include(l => l.User).AsQueryable();
            if (filter.LecturerId.HasValue) lecturers = lecturers.Where(l => l.Id == filter.LecturerId.Value);

            var list = await lecturers.ToListAsync();

            var results = new List<WorkloadAnalysisViewModel>();
            foreach (var l in list)
            {
                var courses = await _context.Courses.Where(c => c.LecturerId == l.Id).ToListAsync();
                var courseIds = courses.Select(c => c.Id).ToList();
                var assignmentsCount = await _context.Assignments.CountAsync(a => courseIds.Contains(a.CourseId) && (!filter.From.HasValue || a.CreatedOn >= filter.From.Value) && (!filter.To.HasValue || a.CreatedOn <= filter.To.Value));
                var submissionsCount = await _context.AssignmentSubmissions.CountAsync(s => courseIds.Contains(s.Assignment.CourseId) && (!filter.From.HasValue || s.SubmittedOn >= filter.From.Value) && (!filter.To.HasValue || s.SubmittedOn <= filter.To.Value));
                var pending = await _context.AssignmentSubmissions.CountAsync(s => courseIds.Contains(s.Assignment.CourseId) && s.Grade == null);

                results.Add(new WorkloadAnalysisViewModel
                {
                    LecturerId = l.Id,
                    LecturerName = l.User.FullName,
                    CoursesCount = courses.Count,
                    AssignmentsCount = assignmentsCount,
                    SubmissionsCount = submissionsCount,
                    PendingGradingCount = pending
                });
            }

            return results;
        }

        public async Task<EnrollmentSummaryViewModel> GetEnrollmentSummaryAsync(ReportFilterViewModel filter)
        {
            var q = _context.Enrollments.Include(e => e.Course).AsQueryable();
            if (filter.CourseId.HasValue) q = q.Where(e => e.CourseId == filter.CourseId.Value);
            if (filter.LecturerId.HasValue) q = q.Where(e => e.Course.LecturerId == filter.LecturerId.Value);
            if (filter.From.HasValue) q = q.Where(e => e.EnrolledOn >= filter.From.Value);
            if (filter.To.HasValue) q = q.Where(e => e.EnrolledOn <= filter.To.Value);

            var list = await q.ToListAsync();

            // group by month for chart-ready data
            var grouped = list.GroupBy(e => new { Year = e.EnrolledOn.Year, Month = e.EnrolledOn.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"), Count = g.Count() })
                .ToList();

            var vm = new EnrollmentSummaryViewModel
            {
                TotalEnrollments = list.Count,
                Labels = grouped.Select(g => g.Label).ToList(),
                Values = grouped.Select(g => g.Count).ToList()
            };

            return vm;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}