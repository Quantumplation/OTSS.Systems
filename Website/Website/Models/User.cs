using Microsoft.AspNet.Identity.EntityFramework;

namespace Website.Models
{
    public class User : IdentityUser
    {
        public InviteKey Invite { get; set; }
    }
}