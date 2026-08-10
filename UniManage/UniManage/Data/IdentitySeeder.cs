using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using UniManage.Helpers;
using UniManage.Models;

namespace UniManage.Data
{
    public static class IdentitySeeder
    {
        public static void Seed(UniManageDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                RequireUniqueEmail = true
            };

            // Create roles
            EnsureRole(roleManager, RoleNames.Administrator);
            EnsureRole(roleManager, RoleNames.Lecturer);
            EnsureRole(roleManager, RoleNames.Student);

            // Create admin user if not exists
            var adminEmail = "admin@unimanage.local";
            var adminUser = userManager.FindByEmail(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator"
                };

                var result = userManager.Create(adminUser, "Admin@12345"); // demo password - require change in real deployment
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ", result.Errors));
                }
            }

            // Ensure admin role
            if (!userManager.IsInRole(adminUser.Id, RoleNames.Administrator))
            {
                userManager.AddToRole(adminUser.Id, RoleNames.Administrator);
            }

            // Create an Administrator profile row if missing
            var adminProfile = context.Administrators.FirstOrDefault(a => a.ApplicationUserId == adminUser.Id);
            if (adminProfile == null)
            {
                context.Administrators.Add(new Administrator
                {
                    ApplicationUserId = adminUser.Id,
                    Office = "Main Office",
                    Title = "Administrator"
                });
                context.SaveChanges();
            }
        }

        private static void EnsureRole(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!roleManager.RoleExists(roleName))
            {
                var role = new IdentityRole(roleName);
                var res = roleManager.Create(role);
                if (!res.Succeeded)
                {
                    throw new Exception(string.Join("; ", res.Errors));
                }
            }
        }
    }
}