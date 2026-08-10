using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using UniManage.Models;

namespace UniManage.Data
{
    public class UniManageDbContext : IdentityDbContext<ApplicationUser>
    {
        public UniManageDbContext()
            : base("DefaultConnection")
        {
            // Use safe initializer for coursework/demo. Switch to MigrateDatabaseToLatestVersion after enabling Migrations.
            Database.SetInitializer(new CreateDatabaseIfNotExists<UniManageDbContext>());
        }

        public static UniManageDbContext Create()
        {
            return new UniManageDbContext();
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Administrator> Administrators { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Grade> Grades { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CoursePrerequisite>()
                .HasKey(cp => new { cp.CourseId, cp.PrerequisiteCourseId });

            modelBuilder.Entity<CoursePrerequisite>()
                .HasRequired(cp => cp.Course)
                .WithMany(c => c.Prerequisites)
                .HasForeignKey(cp => cp.CourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CoursePrerequisite>()
                .HasRequired(cp => cp.PrerequisiteCourse)
                .WithMany()
                .HasForeignKey(cp => cp.PrerequisiteCourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Enrollment>()
                .HasRequired(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Enrollment>()
                .HasRequired(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Assignment>()
                .HasRequired(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .WillCascadeOnDelete(false);
        }
    }
}