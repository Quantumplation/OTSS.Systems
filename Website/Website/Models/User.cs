using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Website.Models
{
    public class User : IdentityUser
    {
        public InviteKey Invite { get; set; }
        public string SlackToken { get; set; }

        public virtual ICollection<Quote> AuthoredQuotes { get; set; }
        public virtual ICollection<Quote> SubmittedQuotes { get; set; }

        public virtual ICollection<LunchVote> LunchVotes { get; set; }
    }
}