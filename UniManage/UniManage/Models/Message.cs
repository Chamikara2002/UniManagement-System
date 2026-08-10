using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string FromUserId { get; set; }

        [Required]
        public string ToUserId { get; set; }

        [ForeignKey("FromUserId")]
        public virtual ApplicationUser FromUser { get; set; }

        [ForeignKey("ToUserId")]
        public virtual ApplicationUser ToUser { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        public string Body { get; set; }

        public DateTime SentOn { get; set; }

        public bool IsRead { get; set; }
    }
}