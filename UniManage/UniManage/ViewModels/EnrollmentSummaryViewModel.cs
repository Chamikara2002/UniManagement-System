using System.Collections.Generic;

namespace UniManage.ViewModels
{
    public class EnrollmentSummaryViewModel
    {
        public int TotalEnrollments { get; set; }
        public List<string> Labels { get; set; }
        public List<int> Values { get; set; }
    }
}