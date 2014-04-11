using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class Database : DbContext
    {
        public Database()
            : base("Azure")
        {
            
        }

        public DbSet<InviteKey> InviteKeys { get; set; }
    }
}