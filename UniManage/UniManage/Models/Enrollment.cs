using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public enum EnrollmentStatus
    {
        Enrolled,
        Waitlisted,
        Dropped
    }

    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        public DateTime EnrolledOn { get; set; }

        public EnrollmentStatus Status { get; set; }
    }
}