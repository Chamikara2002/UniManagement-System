using System;

namespace UniManage.ViewModels
{
    public class ReportFilterViewModel
    {
        public int? CourseId { get; set; }
        public int? LecturerId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Semester { get; set; }

        // Friendly helper to check if any filter provided
        public bool HasFilter => CourseId.HasValue || LecturerId.HasValue || From.HasValue || To.HasValue || !string.IsNullOrEmpty(Semester);
    }
}