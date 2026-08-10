using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        public string FilePath { get; set; }

        public DateTime SubmittedOn { get; set; }

        public string Comments { get; set; }

        public bool IsLate { get; set; }

        public virtual Grade Grade { get; set; }
    }
}