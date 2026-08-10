using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels
{
    public class CreateAssignmentViewModel
    {
        public int CourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int Points { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }
    }
}