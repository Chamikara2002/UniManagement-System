using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UniManage.Data;
using UniManage.Models;
using UniManage.ViewModels;

namespace UniManage.Services
{
    public class AssignmentService : IDisposable
    {
        private readonly UniManageDbContext _context;

        public AssignmentService()
        {
            _context = new UniManageDbContext();
        }

        public async Task<Assignment> GetAssignmentAsync(int assignmentId)
        {
            return await _context.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == assignmentId);
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

        public async Task<AssignmentSubmission> GetSubmissionAsync(int submissionId)
        {
            return await _context.AssignmentSubmissions.Include(s => s.Assignment).Include(s => s.Student).FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<AssignmentSubmission> GetStudentSubmissionAsync(int assignmentId, int studentId)
        {
            return await _context.AssignmentSubmissions.Where(s => s.AssignmentId == assignmentId && s.StudentId == studentId).FirstOrDefaultAsync();
        }

        public async Task<bool> CanStudentSubmitAsync(int studentId, int assignmentId)
        {
            var assignment = await GetAssignmentAsync(assignmentId);
            if (assignment == null) return false;

            var enrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == assignment.CourseId && e.Status == EnrollmentStatus.Enrolled);
            return enrolled;
        }

        public async Task<(bool Success, string Message, AssignmentSubmission Submission)> SubmitAssignmentAsync(int studentId, SubmitAssignmentViewModel model, string filePath)
        {
            var assignment = await GetAssignmentAsync(model.AssignmentId);
            if (assignment == null) return (false, "Assignment not found.", null);

            // Verify enrollment
            var enrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == assignment.CourseId && e.Status == EnrollmentStatus.Enrolled);
            if (!enrolled) return (false, "You are not enrolled in the course for this assignment.", null);

            // Check if already submitted
            var existing = await _context.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == model.AssignmentId && s.StudentId == studentId);
            var isLate = DateTime.UtcNow > assignment.DueDate;

            if (existing is not null)
            {
                // update
                var submissionToUpdate = existing;
                submissionToUpdate.FilePath = filePath ?? submissionToUpdate.FilePath;
                submissionToUpdate.Comments = model.Comments ?? submissionToUpdate.Comments;
                submissionToUpdate.SubmittedOn = DateTime.UtcNow;
                submissionToUpdate.IsLate = isLate;
                await _context.SaveChangesAsync();
                return (true, "Submission updated.", submissionToUpdate);
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = model.AssignmentId,
                StudentId = studentId,
                FilePath = filePath,
                SubmittedOn = DateTime.UtcNow,
                Comments = model.Comments,
                IsLate = isLate
            };
            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return (true, "Submitted successfully.", submission);
        }

        public async Task<(bool Success, string Message)> GradeSubmissionAsync(int submissionId, decimal score, string feedback, string gradedById)
        {
            var submission = await GetSubmissionAsync(submissionId);
            if (submission == null) return (false, "Submission not found.");

            var assignment = submission.Assignment;
            if (assignment == null) return (false, "Assignment details were not found.");
            if (score < 0 || score > assignment.Points)
                return (false, $"Score must be between 0 and {assignment.Points}.");

            var grade = submission.Grade ?? new Grade { AssignmentSubmissionId = submission.Id };
            grade.Score = score;
            grade.Feedback = feedback;
            grade.GradedById = gradedById;
            grade.GradedOn = DateTime.UtcNow;

            if (submission.Grade == null)
                _context.Grades.Add(grade);
            else
                _context.Entry(grade).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return (true, "Graded successfully.");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
