using System.Collections.Generic;
using UniManage.Models;

namespace UniManage.ViewModels
{
    public class LecturerDashboardViewModel
    {
        public ApplicationUser User { get; set; }
        public Lecturer Lecturer { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }
    }
}