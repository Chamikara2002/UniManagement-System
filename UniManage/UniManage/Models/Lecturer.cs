using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class Lecturer
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeNumber { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }

        public Lecturer()
        {
            Courses = new HashSet<Course>();
            Assignments = new HashSet<Assignment>();
        }
    }
}