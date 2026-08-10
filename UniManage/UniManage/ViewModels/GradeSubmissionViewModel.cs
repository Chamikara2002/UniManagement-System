using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels
{
    public class GradeSubmissionViewModel
    {
        public int SubmissionId { get; set; }

        [Range(0, 100)]
        public decimal Score { get; set; }

        public string Feedback { get; set; }
    }
}