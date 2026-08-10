using UniManage.Models;

namespace UniManage.ViewModels
{
    public class AssignmentDetailsViewModel
    {
        public Assignment Assignment { get; set; }
        public bool IsEnrolled { get; set; }
        public AssignmentSubmission StudentSubmission { get; set; }
        public bool CanSubmit { get; set; }
    }
}