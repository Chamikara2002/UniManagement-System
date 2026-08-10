using System.ComponentModel.DataAnnotations;
using System.Web;

namespace UniManage.ViewModels
{
    public class SubmitAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        // File is optional; students may submit text only
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File { get; set; }

        [StringLength(2000)]
        public string Comments { get; set; }
    }
}