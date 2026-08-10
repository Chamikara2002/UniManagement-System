using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UniManage.Data;
using UniManage.Models;
using UniManage.ViewModels;

namespace UniManage.Services
{
    public class StudentService : IDisposable
    {
        private readonly UniManageDbContext _context;

        public StudentService()
        {
            _context = new UniManageDbContext();
        }

        public async Task<Student> GetStudentByUserIdAsync(string userId)
        {
            return await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
        }

        public async Task<StudentDashboardViewModel> GetDashboardAsync(string userId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            if (student == null) return null;

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Enrolled)
                .OrderByDescending(e => e.EnrolledOn)
                .ToListAsync();

            var enrolledCount = enrollments.Count;

            var upcomingAssignmentsCount = await _context.Assignments
                .Where(a => enrollments.Select(e => e.CourseId).Contains(a.CourseId) && DbFunctions.DiffDays(DateTime.UtcNow, a.DueDate) >= 0)
                .CountAsync();

            var recentSubmissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Where(s => s.StudentId == student.Id)
                .OrderByDescending(s => s.SubmittedOn)
                .Take(5)
                .ToListAsync();

            var latestGrades = await _context.Grades
                .Include(g => g.AssignmentSubmission)
                .Where(g => recentSubmissions.Select(s => s.Id).Contains(g.AssignmentSubmissionId))
                .OrderByDescending(g => g.GradedOn)
                .ToListAsync();

            return new StudentDashboardViewModel
            {
                User = student.User,
                Student = student,
                Enrollments = enrollments,
                EnrolledCoursesCount = enrolledCount,
                UpcomingAssignmentsCount = upcomingAssignmentsCount,
                RecentSubmissions = recentSubmissions,
                LatestGrades = latestGrades
            };
        }

        public async Task<List<CourseListItemViewModel>> BrowseCoursesAsync(string userId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            var courses = await _context.Courses
                .Include(c => c.Lecturer)
                .Include("Lecturer.User")
                .Include(c => c.Enrollments)
                .ToListAsync();

            var enrollments = await _context.Enrollments.Where(e => e.StudentId == (student != null ? student.Id : -1)).ToListAsync();

            var list = courses.Select(c => new CourseListItemViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                Credits = c.Credits,
                Capacity = c.Capacity,
                LecturerName = c.Lecturer != null && c.Lecturer.User != null ? c.Lecturer.User.FullName : "TBA",
                EnrolledCount = c.Enrollments != null ? c.Enrollments.Count : 0,
                IsEnrolled = enrollments.Any(e => e.CourseId == c.Id && e.Status == EnrollmentStatus.Enrolled)
            }).ToList();

            return list;
        }

        public async Task<Course> GetCourseDetailsAsync(int courseId, string userId)
        {
            var course = await _context.Courses
                .Include(c => c.Lecturer)
                .Include("Lecturer.User")
                .Include(c => c.Materials)
                .Include("Prerequisites.PrerequisiteCourse")
                .FirstOrDefaultAsync(c => c.Id == courseId);
            return course;
        }

        public async Task<List<Course>> GetMyCoursesAsync(string userId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            if (student == null) return new List<Course>();

            var courses = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.Course)
                .Include(c => c.Lecturer)
                .ToListAsync();

            return courses;
        }

        public async Task<List<Assignment>> GetMyAssignmentsAsync(string userId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            if (student == null) return new List<Assignment>();

            var courseIds = await _context.Enrollments.Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Enrolled).Select(e => e.CourseId).ToListAsync();
            var assignments = await _context.Assignments.Where(a => courseIds.Contains(a.CourseId)).OrderBy(a => a.DueDate).ToListAsync();
            return assignments;
        }

        public async Task<List<Grade>> GetMyGradesAsync(string userId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            if (student == null) return new List<Grade>();

            var submissions = await _context.AssignmentSubmissions.Where(s => s.StudentId == student.Id).Select(s => s.Id).ToListAsync();
            var grades = await _context.Grades.Where(g => submissions.Contains(g.AssignmentSubmissionId)).Include(g => g.AssignmentSubmission).OrderByDescending(g => g.GradedOn).ToListAsync();
            return grades;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
