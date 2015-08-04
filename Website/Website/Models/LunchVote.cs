using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class LunchVote
    {
        public int Id { get; set; }
        [Required]
        [InverseProperty("Votes")]
        public LunchPoll Poll { get; set; }
        [Required]
        [InverseProperty("LunchVotes")]
        public User User { get; set; }
        [Required]
        public LunchOption Option { get; set; }
        public int Score { get; set; }
    }
}