using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class CoursePrerequisite
    {
        // Composite key will be configured in DbContext
        public int CourseId { get; set; }
        public int PrerequisiteCourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("PrerequisiteCourseId")]
        public virtual Course PrerequisiteCourse { get; set; }
    }
}