using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Website.Models;
using Website.ViewModels.Web;

namespace Website.Controllers
{
    [RoutePrefix("User")]
    [Authorize]
    public class UserController : Controller
    {
        public UserController()
            : this(new DatabaseContext())
        {
        }

        public UserController(DatabaseContext context)
            : this(new UserManager<User>(new UserStore<User>(context)))
        {
            DbContext = context;
        }

        public UserController(UserManager<User> userManager)
        {
            UserManager = userManager;
        }


        public UserManager<User> UserManager { get; private set; }

        private DatabaseContext DbContext { get; set; }

        [Route("Login")]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [Route("Login")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel vm, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(vm.UserName, vm.Password);
                if (user != null)
                {
                    await SignInAsync(user, vm.RememberMe);
                    if (!String.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            return View(vm);
        }

        [Route("LogOff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }
        
        [AllowAnonymous]
        [Route("Register")]
        public ActionResult Register()
        {
            return View();
        }


        [Route("Register")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // Check if the invite code is valid
                // Check to make sure the key is valid.
                var matchingKey = await (from key in DbContext.InviteKeys
                    where key.Key == vm.InviteKey
                    select key).ToListAsync();
                if (matchingKey.Any())
                {
                    var user = vm.ToUser();
                    user.Invite = matchingKey.First();
                    var result = await UserManager.CreateAsync(user, vm.Password);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                    AddErrors(result);
                }
                else
                {
                    ModelState.AddModelError("InviteKey", "Invite key is not recognized.");
                }
            }
            return View(vm);
        }
       
        [Route("Manage")]
        public async Task<ActionResult> Manage()
        {
            var viewModel = new ManageUserViewModel();
            using (var db = new DatabaseContext())
            {
                var userId = User.Identity.GetUserId();
                var user = await db.Users.Where(x => x.Id == userId).SingleOrDefaultAsync();
                if (user.SlackToken != null)
                {
                    var userRaw = new WebClient().DownloadString($"https://slack.com/api/auth.test?token={user.SlackToken}");
                    var userResp = JsonConvert.DeserializeAnonymousType(userRaw, new { user = "" });
                    viewModel.SlackUsername = userResp.user;
                }
                return View("Manage", viewModel);
            }
        }

        [Route("LinkWithSlack")]
        public ActionResult LinkWithSlack()
        {
            var clientId = ConfigurationManager.AppSettings["SlackClientId"];
            var teamId = ConfigurationManager.AppSettings["SlackTeamId"];
            if (clientId == null || teamId == null)
                return View("Error");
            return Redirect($"https://slack.com/oauth/authorize?client_id={clientId}&scope=identify&state={User.Identity.GetUserId()}&team={teamId}");
        }

        [Route("Unlink")]
        public async Task<ActionResult> Unlink()
        {
            using (var db = new DatabaseContext())
            {
                var userId = User.Identity.GetUserId();
                var user = await db.Users.Where(x => x.Id == userId).SingleOrDefaultAsync();
                user.SlackToken = null;
                await db.SaveChangesAsync();
                return RedirectToAction("Manage");
            }
        }

        [AllowAnonymous, Route("ConfirmLink")]
        public async Task<ActionResult> ConfirmLink(string code, string state, string error)
        {
            if (error != null)
                return View("Error");
            if (state != User.Identity.GetUserId())
                return View("Error");
            using (var db = new DatabaseContext())
            {
                var user = db.Users.Where(x => x.Id == state).SingleOrDefaultAsync();
                var client = new WebClient();
                var clientId = ConfigurationManager.AppSettings["SlackClientId"];
                var clientSecret = ConfigurationManager.AppSettings["SlackSecret"];
                if (clientId == null || clientSecret == null)
                    return View("Error");
                var tokenRaw = client.DownloadString(new Uri($"https://slack.com/api/oauth.access?client_id={clientId}&client_secret={clientSecret}&code={code}"));
                var token = JsonConvert.DeserializeAnonymousType(tokenRaw, new {access_token = "", scope = ""}).access_token;
                (await user).SlackToken = token;
                await db.SaveChangesAsync();

                return RedirectToAction("Manage");
            }
        }

        [Route("Manage")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(UpdatePasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), vm.OldPassword, vm.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    await SignInAsync(user, isPersistent: false);
                    ViewBag.StatusMessage = "Password changed successfully!";
                    return View();
                }
                else
                {
                    AddErrors(result);
                }
            }
            ViewBag.StatusMessage = "An error occured.";
            return View();
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(User user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
	}
}