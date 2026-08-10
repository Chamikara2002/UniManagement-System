namespace UniManage.ViewModels
{
    public class WorkloadAnalysisViewModel
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; }
        public int CoursesCount { get; set; }
        public int AssignmentsCount { get; set; }
        public int SubmissionsCount { get; set; }
        public int PendingGradingCount { get; set; }
    }
}