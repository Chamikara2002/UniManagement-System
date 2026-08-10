using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string Program { get; set; }

        public int Year { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }

        public Student()
        {
            Enrollments = new HashSet<Enrollment>();
            AssignmentSubmissions = new HashSet<AssignmentSubmission>();
        }
    }
}