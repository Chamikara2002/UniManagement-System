using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using UniManage.Data;
using UniManage.Models;
using UniManage.ViewModels;

namespace UniManage.Services
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string PrimaryRole { get; set; }
        public IdentityResult IdentityResult { get; set; }
        public bool Succeeded => Success && (IdentityResult == null || IdentityResult.Succeeded);
        public System.Collections.Generic.IEnumerable<string> Errors => IdentityResult?.Errors;
    }

    public class AuthService : IDisposable
    {
        private readonly UniManageDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService()
        {
            _context = new UniManageDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        public async Task<AuthResult> RegisterAsync(RegisterViewModel model, HttpContextBase httpContext)
        {
            // Unique email check
            if (_userManager.FindByEmail(model.Email) != null)
            {
                return new AuthResult { Success = false, ErrorMessage = "Email is already registered." };
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = true
            };

            var identityResult = await _userManager.CreateAsync(user, model.Password);
            if (!identityResult.Succeeded)
            {
                return new AuthResult { Success = false, IdentityResult = identityResult };
            }

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            await _userManager.AddToRoleAsync(user.Id, model.Role);

            // Create profile rows
            if (model.Role == "Student")
            {
                _context.Students.Add(new Student { ApplicationUserId = user.Id, StudentNumber = model.StudentNumber });
            }
            else if (model.Role == "Lecturer")
            {
                _context.Lecturers.Add(new Lecturer { ApplicationUserId = user.Id, EmployeeNumber = model.EmployeeNumber });
            }
            else if (model.Role == "Administrator")
            {
                _context.Administrators.Add(new Administrator { ApplicationUserId = user.Id });
            }

            await _context.SaveChangesAsync();

            // Auto sign-in
            var authManager = httpContext.GetOwinContext().Authentication;
            var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            authManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

            return new AuthResult { Success = true, PrimaryRole = model.Role, IdentityResult = identityResult };
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model, HttpContextBase httpContext)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
            }

            if (!user.IsActive)
            {
                return new AuthResult { Success = false, ErrorMessage = "Your account has been deactivated. Contact administrator." };
            }

            // Check password
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
            }

            // Create identity and sign-in
            var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            var authManager = httpContext.GetOwinContext().Authentication;
            authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            authManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, identity);

            // Get primary role
            var roles = await _userManager.GetRolesAsync(user.Id);
            var primaryRole = roles.FirstOrDefault();

            return new AuthResult { Success = true, PrimaryRole = primaryRole };
        }

        public void Logout(HttpContextBase httpContext)
        {
            var authManager = httpContext.GetOwinContext().Authentication;
            authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        public void Dispose()
        {
            _userManager?.Dispose();
            _roleManager?.Dispose();
            _context?.Dispose();
        }
    }
}