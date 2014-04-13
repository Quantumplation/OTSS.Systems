using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Website.Models
{
    public class DatabaseContext : IdentityDbContext<User>
    {
        public DatabaseContext()
            : base(ConfigurationManager.AppSettings["CurrentConnectionString"])
        {
            
        }

        public DbSet<InviteKey> InviteKeys { get; set; }
    }
}