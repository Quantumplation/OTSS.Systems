using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Models;

namespace Website.ViewModels.API
{
    public class LunchOptionViewModel
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string Name { get; set; }
        public int Score { get { return Upvotes.Count - Downvotes.Count; } }

        public ICollection<string> Upvotes { get; set; }
        public ICollection<string> Downvotes { get; set; }

        public LunchOptionViewModel()
        {
            Upvotes = new List<string>();
            Downvotes = new List<string>();
        }

        public LunchOptionViewModel(int pollId, LunchOption lo, IEnumerable<LunchVote> votes)
        {
            Id = lo.Id;
            PollId = pollId;
            Name = lo.Name;

            var vLookup = votes.ToLookup(v => v.Score, v => v.User.UserName);
            Upvotes = vLookup[1].OrderBy(n => n).ToList();
            Downvotes = vLookup[-1].OrderBy(n => n).ToList();
        }
    }
}