using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Website.ViewModels.Web;
using Website.Models;

namespace Website.Hubs
{
    //wish I'd made a Grub Management System...
    public class LunchHub : RazorHub
    {
        public static void OnVote(LunchOption option, IEnumerable<LunchVote> votes)
        {
            OnVote(new LunchOptionViewModel(option, votes));
        }

        public static void OnVote(LunchOptionViewModel model)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LunchHub>();

            var page = Render("_LunchOption", "Lunch", model);

            hubContext.Clients.All.OnVote(model.Id, model.Upvotes, model.Downvotes, page);
        }

    }
}