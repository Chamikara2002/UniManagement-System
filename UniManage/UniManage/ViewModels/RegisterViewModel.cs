using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Student specific
        public string StudentNumber { get; set; }

        // Lecturer specific
        public string EmployeeNumber { get; set; }
    }
}