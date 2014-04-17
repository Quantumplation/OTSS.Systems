using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Website.Models
{
    public class User : IdentityUser
    {
        public InviteKey Invite { get; set; }


        public virtual ICollection<Quote> AuthoredQuotes { get; set; }
        public virtual ICollection<Quote> SubmittedQuotes { get; set; } 
    }
}