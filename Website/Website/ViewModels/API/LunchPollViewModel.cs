using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Models;

namespace Website.ViewModels.API
{
    public class LunchPollViewModel
    {
        public LunchPollViewModel(LunchPoll poll)
        {
            Id = poll.Id;
            Info = new LunchPollInfoViewModel(poll);
            Options = (
                from vote in poll.Votes
                group vote by vote.Option into g
                select new LunchOptionViewModel(poll.Id, g.Key, g)
            ).ToDictionary(o => o.Id);
        }
        public int Id { get; set; }
        public LunchPollInfoViewModel Info { get; set; }
        public IDictionary<int, LunchOptionViewModel> Options { get; set; }
    }
}