using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Website.Models;
using Website.ViewModels.Web;

namespace Website.Controllers.Web
{
    [RoutePrefix("Lunch")]
    [Authorize]
    public class LunchController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View(new LunchPollViewModel(GetDailyPoll(), User.Identity.Name));
        }

        private LunchPoll GetDailyPoll()
        {
            using (var dbContext = new DatabaseContext())
            {
                var now = DateTime.Now.Date;
                var poll = dbContext.LunchPolls
                    .Include(p => p.Votes.Select(v => v.User))
                    .Include(p => p.Votes.Select(v => v.Option))
                    .SingleOrDefault(lp => DbFunctions.TruncateTime(lp.Date) == now);
                if (poll != null) return poll;

                dbContext.LunchPolls.Add(poll = new LunchPoll
                {
                    Date = DateTime.Now,
                    Votes = new List<LunchVote>()
                });
                dbContext.SaveChanges();
                return poll;
            }
        }
    }
}