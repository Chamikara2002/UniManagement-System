using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        public int LecturerId { get; set; }
        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int Points { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime CreatedOn { get; set; }

        public virtual ICollection<AssignmentSubmission> Submissions { get; set; }

        public Assignment()
        {
            Submissions = new HashSet<AssignmentSubmission>();
            CreatedOn = DateTime.UtcNow;
        }
    }
}