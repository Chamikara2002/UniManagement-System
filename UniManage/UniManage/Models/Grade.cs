using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class Grade
    {
        public int Id { get; set; }

        public int AssignmentSubmissionId { get; set; }
        [ForeignKey("AssignmentSubmissionId")]
        public virtual AssignmentSubmission AssignmentSubmission { get; set; }

        public string GradedById { get; set; }

        public decimal Score { get; set; }

        public string Feedback { get; set; }

        public DateTime GradedOn { get; set; }
    }
}