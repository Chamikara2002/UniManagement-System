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
    public class LecturerService : IDisposable
    {
        private readonly UniManageDbContext _context;

        public LecturerService()
        {
            _context = new UniManageDbContext();
        }

        public async Task<Lecturer> GetLecturerByUserIdAsync(string userId)
        {
            return await _context.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.ApplicationUserId == userId);
        }

        public async Task<LecturerDashboardViewModel> GetDashboardAsync(string userId)
        {
            var lecturer = await GetLecturerByUserIdAsync(userId);
            if (lecturer == null) return null;

            var courses = await _context.Courses.Where(c => c.LecturerId == lecturer.Id).ToListAsync();
            var courseIds = courses.Select(c => c.Id).ToList();

            var assignments = await _context.Assignments.Where(a => courseIds.Contains(a.CourseId)).ToListAsync();
            var activeAssignments = assignments.Where(a => a.DueDate >= DateTime.UtcNow).ToList();

            var pendingSubs = await _context.AssignmentSubmissions.Where(s => courseIds.Contains(s.Assignment.CourseId) && s.Grade == null).Include(s => s.Assignment).ToListAsync();

            var recentSubs = pendingSubs.OrderByDescending(s => s.SubmittedOn).Take(10).ToList();

            return new LecturerDashboardViewModel
            {
                User = lecturer.User,
                Lecturer = lecturer,
                AssignedCoursesCount = courses.Count,
                ActiveAssignmentsCount = activeAssignments.Count,
                PendingSubmissionsCount = pendingSubs.Count,
                AssignedCourses = courses,
                ActiveAssignments = activeAssignments,
                RecentSubmissions = recentSubs
            };
        }

        public async Task<List<Course>> GetMyCoursesAsync(string userId)
        {
            var lecturer = await GetLecturerByUserIdAsync(userId);
            if (lecturer == null) return new List<Course>();
            return await _context.Courses.Where(c => c.LecturerId == lecturer.Id).Include(c => c.Prerequisites).ToListAsync();
        }

        public async Task<Course> GetCourseDetailsAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Materials)
                .Include(c => c.Assignments)
                .Include("Assignments.Submissions.Student.User")
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<bool> UploadMaterialAsync(int courseId, string title, string filePath, string uploadedById)
        {
            var material = new CourseMaterial
            {
                CourseId = courseId,
                Title = title,
                FilePath = filePath,
                UploadedById = uploadedById,
                UploadedOn = DateTime.UtcNow
            };
            _context.CourseMaterials.Add(material);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Assignment> CreateAssignmentAsync(CreateAssignmentViewModel model, int lecturerId)
        {
            var assignment = new Assignment
            {
                CourseId = model.CourseId,
                LecturerId = lecturerId,
                Title = model.Title,
                Description = model.Description,
                Points = model.Points,
                DueDate = model.DueDate,
                CreatedOn = DateTime.UtcNow
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<List<Assignment>> GetAssignmentsForCourseAsync(int courseId)
        {
            return await _context.Assignments.Where(a => a.CourseId == courseId).OrderByDescending(a => a.CreatedOn).ToListAsync();
        }

        public async Task<List<AssignmentSubmission>> GetSubmissionsForAssignmentAsync(int assignmentId)
        {
            return await _context.AssignmentSubmissions.Where(s => s.AssignmentId == assignmentId).Include(s => s.Student).Include(s => s.Assignment).ToListAsync();
        }

        public async Task<AssignmentSubmission> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include("Student.User")
                .Include(s => s.Assignment)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<bool> GradeSubmissionAsync(int submissionId, decimal score, string feedback, string gradedById)
        {
            var submission = await _context.AssignmentSubmissions.Include(s => s.Grade).FirstOrDefaultAsync(s => s.Id == submissionId);
            if (submission == null) return false;

            var grade = submission.Grade ?? new Grade { AssignmentSubmissionId = submission.Id };
            grade.Score = score;
            grade.Feedback = feedback;
            grade.GradedById = gradedById;
            grade.GradedOn = DateTime.UtcNow;

            if (submission.Grade == null)
            {
                _context.Grades.Add(grade);
            }
            else
            {
                _context.Entry(grade).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetReportsAsync(int lecturerId)
        {
            // Simple placeholder reports: count per course
            var courses = await _context.Courses.Where(c => c.LecturerId == lecturerId).ToListAsync();
            var report = courses.Select(c => new
            {
                CourseId = c.Id,
                c.Title,
                Enrolled = c.Enrollments?.Count ?? 0,
                Assignments = c.Assignments?.Count ?? 0
            }).ToList();

            return report;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}