namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Last_Dance_System.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Create roles
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            // Create Administrator role
            if (!roleManager.RoleExists("Administrator"))
            {
                roleManager.Create(new IdentityRole("Administrator"));
            }

            // Create Student role
            if (!roleManager.RoleExists("Student"))
            {
                roleManager.Create(new IdentityRole("Student"));
            }

            // Create UserManager
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            // Admin login details
            string adminEmail = "admin@tkdrivingschool.co.za";
            string adminPassword = "Admin@12345";

            // Check if admin already exists
            var admin = userManager.FindByEmail(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = userManager.Create(admin, adminPassword);

                if (result.Succeeded)
                {
                    userManager.AddToRole(admin.Id, "Administrator");
                }
            }
            else
            {
                // Make sure existing admin has Administrator role
                if (!userManager.IsInRole(admin.Id, "Administrator"))
                {
                    userManager.AddToRole(admin.Id, "Administrator");
                }
            }
        }
    }
}