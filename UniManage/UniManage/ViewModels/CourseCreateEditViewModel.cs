using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels
{
    public class CourseCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int Credits { get; set; }

        public int Capacity { get; set; }

        public int? LecturerId { get; set; }
    }
}