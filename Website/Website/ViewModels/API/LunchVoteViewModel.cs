using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.ViewModels.API
{
    public class LunchVoteViewModel
    {
        public int PollId { get; set; }
        public string OptionName { get; set; }
        public int Score { get; set; }
    }
}