using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Website.Models;

namespace Website.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Website.Models.DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Website.Models.DatabaseContext";
        }

        protected override void Seed(Website.Models.DatabaseContext context)
        {
            var adminUser = new User
            {
                UserName = "Admin",
                Email = "pi.lanningham@nyu.edu",
                EmailConfirmed = true,
            };

            var manager = new UserManager<User>(new UserStore<User>(new Website.Models.DatabaseContext()));
            manager.Create(adminUser, Crypto.SHA256("password"));
        }
    }
}
