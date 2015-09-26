using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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

        [Authorize(Roles = "Lunch Administrator")]
        [HttpGet, Route("Decision")]
        public ActionResult EnterDecision()
        {
            using (var dbContext = new DatabaseContext())
            {
                var polls = LunchAPI.GetPolls(dbContext, DateTime.Now);
                return View(new EnterLunchDecisionViewModel(polls));
            }
        }

        [Authorize(Roles = "Lunch Administrator")]
        [HttpPost, Route("Decision")]
        public async Task<ActionResult> EnterDecision(EnterLunchDecisionViewModel model)
        {
            var result = await LunchAPI.Decide(model.SelectedPollId, model.Decision);
            var success = (await result.ExecuteAsync(new CancellationToken())).IsSuccessStatusCode;

            return success
                ? RedirectToAction("Index")
                : EnterDecision();
        }
    }
}