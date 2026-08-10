using System.Collections.Generic;
using UniManage.Models;

namespace UniManage.ViewModels
{
    public class StudentDashboardViewModel
    {
        public ApplicationUser User { get; set; }
        public Student Student { get; set; }

        // Summary
        public int EnrolledCoursesCount { get; set; }
        public int UpcomingAssignmentsCount { get; set; }

        // Details
        public IEnumerable<Enrollment> Enrollments { get; set; }
        public IEnumerable<AssignmentSubmission> RecentSubmissions { get; set; }
        public IEnumerable<Grade> LatestGrades { get; set; }
    }
}