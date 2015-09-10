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
            Name = poll.Name;
            Voters = poll.Voters.Select(u => u.UserName).ToList();
            Options = new List<LunchOptionViewModel>
            (
                from vote in poll.Votes
                group vote by vote.Option into g
                select new LunchOptionViewModel(poll.Id, g.Key, g)
            );
        }

        public LunchPollViewModel(LunchPoll poll, string username)
        {
            Id = poll.Id;
            Name = poll.Name;
            Voters = poll.Voters.Select(u => u.UserName).ToList();
            ContainsCurrentUser = Voters.Contains(username);
            Options = new List<LunchOptionViewModel>
            (
                from vote in poll.Votes
                group vote by vote.Option into g
                select new LunchOptionViewModel(poll.Id, g.Key, g, username, ContainsCurrentUser)
            );
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<string> Voters { get; set; }
        public ICollection<LunchOptionViewModel> Options { get; set; }
        public bool ContainsCurrentUser { get; set; }
    }
}