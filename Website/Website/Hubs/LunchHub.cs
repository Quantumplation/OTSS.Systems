using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Website.ViewModels.API;
using Website.Models;

namespace Website.Hubs
{
    //wish I'd made a Grub Management System...
    public class LunchHub : RazorHub
    {
        public static void OnVote(int pollId, LunchOption option, IEnumerable<LunchVote> votes)
        {
            OnVote(pollId, new LunchOptionViewModel(option, votes));
        }

        public static void OnVote(int pollId, LunchOptionViewModel model)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();

            var page = Render("_LunchOption", "Lunch", model);

            hubContext.Clients.All.OnVote(pollId, model.Id, model.Upvotes, model.Downvotes, page);
        }

        public static void OnPollAdded(LunchPollViewModel model)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();

            var page = Render("_NavbarPoll", "Lunch", model);

            hubContext.Clients.All.OnPollAdded(model.Id, page);
        }
    }
}