using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    // ApplicationUser extends IdentityUser for additional profile fields
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        // If false, user is not allowed to login
        public bool IsActive { get; set; } = true;

        // Convenience property
        public string FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? UserName
            : $"{FirstName} {LastName}";
    }
}
