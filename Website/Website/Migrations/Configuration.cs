using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Website.Models;

namespace Website.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Website.Models.UserDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Website.Models.UserDbContext";
        }

        protected override void Seed(Website.Models.UserDbContext context)
        {
            var adminUser = new User
            {
                UserName = "Quantumplation",
                Email = "pi.lanningham@nyu.edu",
                EmailConfirmed = true,
            };

            var rob = new User
            {
                UserName = "Rob",
                Email = "rob@kelle.tt",
                EmailConfirmed = true,
            };

            var manager = new UserManager<User>(new UserStore<User>(new UserDbContext()));
            manager.Create(adminUser, "password");
            manager.Create(rob, "password");
        }
    }
}
