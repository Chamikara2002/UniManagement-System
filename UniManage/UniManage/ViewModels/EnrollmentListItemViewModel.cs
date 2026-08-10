using System;
using UniManage.Models;

namespace UniManage.ViewModels
{
    public class EnrollmentListItemViewModel
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public DateTime EnrolledOn { get; set; }
        public EnrollmentStatus Status { get; set; }
        public int SeatsLeft { get; set; }
    }
}