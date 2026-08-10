using System;

namespace UniManage.ViewModels
{
    public class CourseListItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string LecturerName { get; set; }
        public int Credits { get; set; }
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public int SeatsLeft => Math.Max(0, Capacity - EnrolledCount);
        public bool IsEnrolled { get; set; }
    }
}