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
            OnVote(new LunchOptionViewModel(pollId, option, votes));
        }

        public static void OnVote(LunchOptionViewModel model)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();

            var view = Render("_LunchOption", "Lunch", model);

            hubContext.Clients.All.OnVote(model, view);
        }

        public static void OnPollChanged(LunchPollViewModel model)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();

            var view = Render("_NavbarPoll", "Lunch", model);

            hubContext.Clients.All.OnPollChanged(model, view);
        }
    }
}