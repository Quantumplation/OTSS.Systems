using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Models;

namespace Website.ViewModels.Web
{
    public class LunchPollViewModel
    {
        public LunchPollViewModel(LunchPoll poll, string username)
        {
            Id = poll.Id;
            Options = new List<LunchOptionViewModel>
            (
                from vote in poll.Votes
                group vote by vote.Option into g
                select new LunchOptionViewModel(g.Key, g, username)
            );
        }
        public int Id { get; set; }
        public ICollection<LunchOptionViewModel> Options { get; set; }
    }
}