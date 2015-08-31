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
        private readonly API.LunchController LunchAPI = new API.LunchController();
         
        [Route("")]
        public async Task<ActionResult> Index()
        {
            return View((await LunchAPI.GetPolls(DateTime.Now)).ToList());
        }

        [Route("{id}", Name = "Poll")]
        public async Task<ActionResult> Poll(int id)
        {
            var poll = await LunchAPI.GetPoll(id);
            return poll != null
                ? (ActionResult)PartialView(poll)
                : HttpNotFound();
        }

        [Route("", Name = "CreatePoll")]
        [HttpPost]
        public async Task<ActionResult> CreatePoll(NewLunchPollViewModel model)
        {
            await LunchAPI.CreatePoll(model.Name);
            return RedirectToAction("Index");
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
                poll.Decision = await LunchAPI.GetOrAddOption(dbContext, option.Name);
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