using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using UniManage.Data;
using UniManage.Models;
using UniManage.ViewModels;

namespace UniManage.Services
{
    public class AdminService : IDisposable
    {
        private readonly UniManageDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService()
        {
            _context = new UniManageDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            _role_manager_init_fallback();
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var totalUsers = await _context.Users.CountAsync();
            var totalStudents = await _context.Students.CountAsync();
            var totalLecturers = await _context.Lecturers.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalEnrollments = await _context.Enrollments.CountAsync();

            var recentCourses = await _context.Courses.OrderByDescending(c => c.CreatedOn).Take(10).ToListAsync();
            var recentUsers = await _context.Users.OrderByDescending(u => u.Id).Take(10).ToListAsync();

            return new AdminDashboardViewModel
            {
                User = user,
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalLecturers = totalLecturers,
                TotalCourses = totalCourses,
                TotalEnrollments = totalEnrollments,
                RecentCourses = recentCourses,
                RecentUsers = recentUsers
            };
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.UserName).ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            var existing = await _userManager.FindByIdAsync(user.Id);
            if (existing == null) return false;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.IsActive = user.IsActive;
            var result = await _userManager.UpdateAsync(existing);
            return result.Succeeded;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.Include(c => c.Lecturer).ToListAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _context.Courses.Include(c => c.Lecturer).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> CreateCourseAsync(Course model)
        {
            model.CreatedOn = DateTime.UtcNow;
            _context.Courses.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> UpdateCourseAsync(Course model)
        {
            var existing = await _context.Courses.FindAsync(model.Id);
            if (existing == null) return false;
            existing.Title = model.Title;
            existing.Code = model.Code;
            existing.Description = model.Description;
            existing.Credits = model.Credits;
            existing.Capacity = model.Capacity;
            existing.LecturerId = model.LecturerId;
            existing.IsActive = model.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var existing = await _context.Courses.FindAsync(id);
            if (existing == null) return false;
            _context.Courses.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignLecturerAsync(int courseId, int lecturerId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;
            course.LecturerId = lecturerId;
            await _context.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            _userManager?.Dispose();
            _roleManager?.Dispose();
            _context?.Dispose();
        }

        private void _role_manager_init_fallback() { }
    }
}