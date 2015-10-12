using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class LunchPoll
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public ICollection<User> Voters { get; set; }
        public virtual ICollection<LunchVote> Votes { get; set; }

        public LunchOption Decision { get; set; }
    }
}