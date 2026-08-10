using System.Collections.Generic;
using UniManage.Models;

namespace UniManage.ViewModels
{
    public class AdminDashboardViewModel
    {
        public ApplicationUser User { get; set; }

        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }

        public IEnumerable<Course> RecentCourses { get; set; }
        public IEnumerable<ApplicationUser> RecentUsers { get; set; }
    }
}
