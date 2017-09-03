namespace ThunderITforGEA.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using ThunderITforGEA.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ThunderITforGEA.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "ThunderITforGEA.Models.ApplicationDbContext";
        }

        protected override void Seed(ThunderITforGEA.Models.ApplicationDbContext context)
        {
            if (!context.Roles.Any(r => r.Name == "admin"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "admin" };

                manager.Create(role);
            }
            if (!context.Roles.Any(r => r.Name == "user"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "user" };

                manager.Create(role);
            }
            if (!context.Users.Any(u => u.UserName == "root"))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser { UserName = "root" };

                manager.Create(user, "7Mm6H$@HhGPrqnCn*96zœ!");
                manager.AddToRole(user.Id, "admin");
            }
        }
    }
}
