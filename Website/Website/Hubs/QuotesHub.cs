using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using Website.Models;
using Website.ViewModels.Web;

namespace Website.Hubs
{
    public class QuotesHub : RazorHub
    {
        private static Lazy<IHubContext> _context = new Lazy<IHubContext>(
            () => GlobalHost.ConnectionManager.GetHubContext<QuotesHub>());

        public static void NewQuote(Quote q)
        {
            var quoteVM = new QuoteViewModel(q);
            var page = Render("_Quote", "Quotes", quoteVM);
            _context.Value.Clients.All.newQuote(page);

            var wc = new WebClient();
            var jsonObject = new JObject();
            jsonObject["channel"] = "#banter";
            jsonObject["username"] = "OTSS.Quotes";
            jsonObject["text"] = $"> {q.Text}\n - {quoteVM.Author} {q.CreatedAt.Year}\n(Created by {q.Submitter.UserName} {q.CreatedAt.ToPrettyInterval()}";
            jsonObject["icon_emoji"] = ":kappa:";
            wc.UploadString(ConfigurationManager.ConnectionStrings["Slack-Banter"].ConnectionString, "POST", jsonObject.ToString());
        }
    }
}