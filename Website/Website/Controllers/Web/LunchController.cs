using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
            using (var dbContext = new DatabaseContext())
            {
                return View(new LunchPollViewModel(GetDailyPoll(dbContext), User.Identity.Name));
            }
        }

        [Authorize(Roles = "Lunch Administrator")]
        [HttpGet, Route("Decision")]
        public ActionResult EnterDecision()
        {
            return View();
        }

        [Authorize(Roles = "Lunch Administrator")]
        [HttpPost, Route("Decision")]
        public async Task<ActionResult> EnterDecision(LunchOption option)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = GetDailyPoll(dbContext);
                poll.Decision = await new API.LunchController().GetOrAddOption(dbContext, option.Name);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }

        private LunchPoll GetDailyPoll(DatabaseContext dbContext)
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