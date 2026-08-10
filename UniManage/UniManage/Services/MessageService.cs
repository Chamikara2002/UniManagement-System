using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UniManage.Data;
using UniManage.Models;
using UniManage.ViewModels;

namespace UniManage.Services
{
    public class MessageService : IDisposable
    {
        private readonly UniManageDbContext _context;

        public MessageService()
        {
            _context = new UniManageDbContext();
        }

        public async Task<Message> SendMessageAsync(string fromUserId, ComposeMessageViewModel model)
        {
            // ensure recipient exists
            var toUser = await _context.Users.FindAsync(model.ToUserId);
            if (toUser == null) throw new InvalidOperationException("Recipient not found.");

            var msg = new Message
            {
                FromUserId = fromUserId,
                ToUserId = model.ToUserId,
                Subject = model.Subject,
                Body = model.Body,
                SentOn = DateTime.UtcNow,
                IsRead = false
            };
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();
            return msg;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.UserName).ToListAsync();
        }

        public async Task<(bool Allowed, string Message)> ValidateRecipientForSenderAsync(string senderUserId, string recipientUserId)
        {
            // If same user, disallow
            if (senderUserId == recipientUserId) return (false, "Cannot send message to yourself.");

            // If sender is student, recipient must be a lecturer of one of their enrolled courses
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == senderUserId);
            if (student != null)
            {
                var lecturerIds = await _context.Enrollments
                    .Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Enrolled)
                    .Select(e => e.Course.Lecturer.ApplicationUserId)
                    .Distinct()
                    .ToListAsync();

                if (lecturerIds.Contains(recipientUserId)) return (true, null);
                return (false, "Students can only message lecturers of their enrolled courses.");
            }

            // If sender is lecturer, recipient must be a student in their courses
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.ApplicationUserId == senderUserId);
            if (lecturer != null)
            {
                var studentIds = await _context.Courses
                    .Where(c => c.LecturerId == lecturer.Id)
                    .SelectMany(c => c.Enrollments)
                    .Where(e => e.Status == EnrollmentStatus.Enrolled)
                    .Select(e => e.Student.ApplicationUserId)
                    .Distinct()
                    .ToListAsync();

                if (studentIds.Contains(recipientUserId)) return (true, null);
                return (false, "Lecturers can only message students enrolled in their courses.");
            }

            // Admins and others may message anyone (for coursework)
            return (true, null);
        }

        public async Task<List<Message>> GetInboxAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.FromUser)
                .Where(m => m.ToUserId == userId)
                .OrderByDescending(m => m.SentOn)
                .ToListAsync();
        }

        public async Task<List<Message>> GetSentAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.ToUser)
                .Where(m => m.FromUserId == userId)
                .OrderByDescending(m => m.SentOn)
                .ToListAsync();
        }

        public async Task<Message> GetMessageAsync(int id, string userId)
        {
            var msg = await _context.Messages.Include(m => m.FromUser).Include(m => m.ToUser).FirstOrDefaultAsync(m => m.Id == id);
            if (msg == null) return null;
            // Only allow participants to view
            if (msg.FromUserId != userId && msg.ToUserId != userId) return null;
            return msg;
        }

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var msg = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.ToUserId == userId);
            if (msg == null) return false;
            msg.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ApplicationUser>> GetPotentialRecipientsForStudentAsync(string studentUserId)
        {
            // students can message lecturers of their enrolled courses
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == studentUserId);
            if (student == null) return new List<ApplicationUser>();

            var lecturerIds = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.Course.Lecturer.ApplicationUserId)
                .Distinct()
                .ToListAsync();

            var users = await _context.Users.Where(u => lecturerIds.Contains(u.Id)).ToListAsync();
            return users;
        }

        public async Task<List<ApplicationUser>> GetPotentialRecipientsForLecturerAsync(string lecturerUserId)
        {
            // lecturers can message students in their own courses
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.ApplicationUserId == lecturerUserId);
            if (lecturer == null) return new List<ApplicationUser>();

            var studentIds = await _context.Courses
                .Where(c => c.LecturerId == lecturer.Id)
                .SelectMany(c => c.Enrollments)
                .Where(e => e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.Student.ApplicationUserId)
                .Distinct()
                .ToListAsync();

            var users = await _context.Users.Where(u => studentIds.Contains(u.Id)).ToListAsync();
            return users;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}