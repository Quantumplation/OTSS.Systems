using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IHttpActionResult = System.Web.Http.IHttpActionResult;
using Website.Models;
using Website.ViewModels.Web;

namespace Website.Controllers.Web
{
    [RoutePrefix("Lunch")]
    [Authorize]
    public class LunchController : Controller
    {
        private readonly API.LunchController LunchAPI;

        public LunchController()
        {
            LunchAPI = DependencyResolver.Current.GetService<API.LunchController>();
            LunchAPI.Request = new System.Net.Http.HttpRequestMessage();
        }
         
        [Route("")]
        public async Task<ActionResult> Index()
        {
            return View((await LunchAPI.GetPolls(DateTime.Now)).ToList());
        }

        [Authorize(Roles = "Lunch Administrator")]
        [HttpGet, Route("Management")]
        public ActionResult Management(LunchAdministrationTaskEnum task = LunchAdministrationTaskEnum.EnterLunchDecision)
        {
            using (var dbContext = new DatabaseContext())
            {
                var polls = LunchAPI.GetPolls(dbContext, DateTime.Now);
                return View("Management", new LunchAdministrationViewModel(polls, task));
            }
        }

        [Authorize(Roles = "Lunch Administrator")]
        [Route(nameof(LunchAdministrationTaskEnum.EnterLunchDecision))]
        public Task<ActionResult> EnterLunchDecision(LunchAdministrationViewModel model)
        {
            return Do(LunchAPI.Decide(model.PollId, model.OptionName));
        }

        [Authorize(Roles = "Lunch Administrator")]
        [Route(nameof(LunchAdministrationTaskEnum.AddUserToLunch))]
        public Task<ActionResult> AddUserToLunch(LunchAdministrationViewModel model)
        {
            return Do(LunchAPI.AddToPoll(model.PollId, model.UserName));
        }

        [Authorize(Roles = "Lunch Administrator")]
        [Route(nameof(LunchAdministrationTaskEnum.RemoveUserFromLunch))]
        public Task<ActionResult> RemoveUserFromLunch(LunchAdministrationViewModel model)
        {
            return Do(LunchAPI.RemoveFromPoll(model.PollId, model.UserName));
        }

        [Authorize(Roles = "Lunch Administrator")]
        [Route(nameof(LunchAdministrationTaskEnum.RenameOption))]
        public Task<ActionResult> RenameOption(LunchAdministrationViewModel model)
        {
            return Do(LunchAPI.RenameOption(model.OptionName, model.NewOptionName));
        }

        [Authorize(Roles = "Lunch Administrator")]
        [Route(nameof(LunchAdministrationTaskEnum.DeleteOption))]
        public Task<ActionResult> DeleteOption(LunchAdministrationViewModel model)
        {
            return Do(LunchAPI.DeleteOption(model.OptionName));
        }

        [Authorize(Roles = "Lunch Administrator")]
        [Route(nameof(LunchAdministrationTaskEnum.DeletePoll))]
        public Task<ActionResult> DeletePoll(LunchAdministrationViewModel model)
        {
            return Do(LunchAPI.DeletePoll(model.PollId));
        }

        private async Task<ActionResult> Do(Task<IHttpActionResult> action)
        {
            var result = await (await action).ExecuteAsync(new CancellationToken());
            if (result.IsSuccessStatusCode)
                return RedirectToAction("Index");
            ModelState.AddModelError("", result.ReasonPhrase);

            LunchAdministrationTaskEnum task;
            var taskName = ControllerContext.RouteData.Values["action"].ToString();
            if (!Enum.TryParse(taskName, out task))
                task = LunchAdministrationTaskEnum.EnterLunchDecision;
            return Management(task);
        }
    }
}