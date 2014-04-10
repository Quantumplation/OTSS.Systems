using Microsoft.AspNet.Identity.EntityFramework;

namespace Website.Models
{
    public class User : IdentityUser
    {
    }

    public class UserDbContext : IdentityDbContext<User>
    {
        public UserDbContext()
            : base("Azure")
        {
        }
    }
}