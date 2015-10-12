using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Models;

namespace Website.ViewModels.API
{
    public class LunchPollInfoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<string> Voters { get; set; }

        public LunchPollInfoViewModel(LunchPoll poll)
        {
            Id = poll.Id;
            Name = poll.Name;
            Voters = poll.Voters.Select(u => u.UserName).ToList();
        }
    }
}