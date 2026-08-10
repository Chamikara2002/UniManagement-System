using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels
{
    public class ComposeMessageViewModel
    {
        [Required]
        public string ToUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        public string Body { get; set; }
    }
}