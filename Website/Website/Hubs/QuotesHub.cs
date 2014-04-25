using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Razor;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using RazorEngine;
using Website.Models;
using Website.ViewModels.Web;

namespace Website.Hubs
{
    public class QuotesHub : Hub
    {
        private static Lazy<IHubContext> _context = new Lazy<IHubContext>(
            () => GlobalHost.ConnectionManager.GetHubContext<QuotesHub>());

        private const string ViewName = "Quotes/_Quote";
        private static readonly string ViewPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", ViewName + ".cshtml");

        public static void NewQuote(Quote q)
        {
            var quoteVM = new QuoteViewModel(q);
            var rawPage = File.ReadAllText(ViewPath);
            var page = Razor.Parse(rawPage, quoteVM);
            _context.Value.Clients.All.newQuote(page);

            var wc = new WebClient();
            var jsonObject = new JObject();
            jsonObject["channel"] = "#banter";
            jsonObject["username"] = "OTSS.Quotes";
            jsonObject["text"] = "> " + q.Text + "\n - " + quoteVM.Author + " " + q.CreatedAt.Year;
            jsonObject["icon_emoji"] = ":kappa:";
            wc.UploadString(ConfigurationManager.ConnectionStrings["Slack-Banter"].ConnectionString, "POST", jsonObject.ToString());
        }
    }
}