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
    public class LunchHub : Hub
    {
        public static void OnVote(int pollId, LunchOption option, IEnumerable<LunchVote> votes)
        {
            OnVote(new LunchOptionViewModel(pollId, option, votes));
        }

        public static void OnVote(LunchOptionViewModel model)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();
            hubContext.Clients.All.OnVote(model);
        }

        public static void OnPollChanged(LunchPollViewModel model)
        {
            if (model.Id == 0) return;
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();
            hubContext.Clients.All.OnPollChanged(model);
        }

        public static void OnOptionDeleted(int id)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();
            hubContext.Clients.All.OnOptionDeleted(id);
        }

        public static void OnPollDeleted(int id)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();
            hubContext.Clients.All.OnPollDeleted(id);
        }
    }
}