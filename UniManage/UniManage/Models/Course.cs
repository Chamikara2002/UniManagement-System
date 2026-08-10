using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int Credits { get; set; }

        public int Capacity { get; set; }

        // If false course is not active and cannot be enrolled
        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; }

        public int? LecturerId { get; set; }

        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; }

        public virtual ICollection<CoursePrerequisite> Prerequisites { get; set; }
        public virtual ICollection<CourseMaterial> Materials { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }

        public Course()
        {
            Prerequisites = new HashSet<CoursePrerequisite>();
            Materials = new HashSet<CourseMaterial>();
            Enrollments = new HashSet<Enrollment>();
            Assignments = new HashSet<Assignment>();
            CreatedOn = DateTime.UtcNow;
        }
    }
}