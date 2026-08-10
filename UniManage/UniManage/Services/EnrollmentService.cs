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
    public class EnrollmentService : IDisposable
    {
        private readonly UniManageDbContext _context;

        public EnrollmentService()
        {
            _context = new UniManageDbContext();
        }

        public async Task<Student> GetStudentByUserIdAsync(string userId)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
        }

        public async Task<Course> GetCourseAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Prerequisites.Select(p => p.PrerequisiteCourse))
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<EnrollResultViewModel> EnrollStudentAsync(string userId, int courseId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            if (student == null) return EnrollResultViewModel.Failure("Student profile not found.");

            var course = await GetCourseAsync(courseId);
            if (course == null) return EnrollResultViewModel.Failure("Course not found.");

            if (!course.IsActive)
                return EnrollResultViewModel.Failure("Cannot enroll: course is not active.");

            // Duplicate enrollment check
            var existing = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == courseId && e.Status == EnrollmentStatus.Enrolled);
            if (existing != null)
                return EnrollResultViewModel.Failure("You are already enrolled in this course.");

            // Prerequisite check: require student to be enrolled in all prerequisite courses (simple rule)
            var prereqs = course.Prerequisites?.Select(p => p.PrerequisiteCourseId).ToList() ?? new List<int>();
            if (prereqs.Any())
            {
                var studentCourseIds = await _context.Enrollments.Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Enrolled).Select(e => e.CourseId).ToListAsync();
                var missing = prereqs.Except(studentCourseIds).ToList();
                if (missing.Any())
                {
                    // provide friendly list of missing course codes if possible
                    var missingCourses = await _context.Courses.Where(c => missing.Contains(c.Id)).Select(c => c.Code).ToListAsync();
                    var missingText = string.Join(", ", missingCourses);
                    return EnrollResultViewModel.Failure($"Cannot enroll: missing prerequisites ({missingText}).");
                }
            }

            // Capacity check
            var enrolledCount = course.Enrollments?.Count(e => e.Status == EnrollmentStatus.Enrolled) ?? 0;
            if (course.Capacity > 0 && enrolledCount >= course.Capacity)
            {
                return EnrollResultViewModel.Failure("Cannot enroll: course is full.");
            }

            // Create enrollment
            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = courseId,
                EnrolledOn = DateTime.UtcNow,
                Status = EnrollmentStatus.Enrolled
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return EnrollResultViewModel.Success(enrollment.Id);
        }

        public async Task<EnrollResultViewModel> DropEnrollmentAsync(string userId, int courseId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            if (student == null) return EnrollResultViewModel.Failure("Student profile not found.");

            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == courseId && e.Status == EnrollmentStatus.Enrolled);
            if (enrollment == null) return EnrollResultViewModel.Failure("You are not enrolled in this course.");

            // Mark as dropped
            enrollment.Status = EnrollmentStatus.Dropped;
            await _context.SaveChangesAsync();
            return EnrollResultViewModel.Success(enrollment.Id);
        }

        public async Task<List<EnrollmentListItemViewModel>> GetMyEnrollmentsAsync(string userId)
        {
            var student = await GetStudentByUserIdAsync(userId);
            if (student == null) return new List<EnrollmentListItemViewModel>();

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledOn)
                .ToListAsync();

            return enrollments.Select(e => new EnrollmentListItemViewModel
            {
                EnrollmentId = e.Id,
                CourseId = e.CourseId,
                CourseCode = e.Course?.Code,
                CourseTitle = e.Course?.Title,
                EnrolledOn = e.EnrolledOn,
                Status = e.Status,
                SeatsLeft = (e.Course != null) ? Math.Max(0, e.Course.Capacity - (e.Course.Enrollments?.Count(x => x.Status == EnrollmentStatus.Enrolled) ?? 0)) : 0
            }).ToList();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
