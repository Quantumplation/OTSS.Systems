using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Website.Models;
using Website.ViewModels.Web;

namespace Website.Controllers.Web
{
    [RoutePrefix("Admin")]
    public class AdminController : Controller
    {
        public AdminController()
            : this(new DatabaseContext())
        {
        }

        public AdminController(DatabaseContext context)
            : this(new UserManager<User>(new UserStore<User>(context)))
        {
            DbContext = context;
        }

        public AdminController(UserManager<User> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<User> UserManager { get; private set; }

        private DatabaseContext DbContext { get; set; }


        [Authorize(Roles = "Administrator, Lunch Administrator")]
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [Route("AddRole")]
        public ActionResult AddRole()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [Route("ResetPassword")]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [Route("DeleteUser")]
        public ActionResult DeleteUser()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost, Route("AddRole")]
        public async Task<ActionResult> AddRole(AddRoleViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(vm.Username);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found");
                    return View();
                }
                var result = await UserManager.AddClaimAsync(user.Id, new Claim(ClaimTypes.Role, vm.Role));
                if (WasSuccessful(result)) return View("Index");
            }
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost, Route("ResetPassword")]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(vm.Username);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found");
                    return View();
                }
                var reset = await UserManager.RemovePasswordAsync(user.Id);
                if (!WasSuccessful(reset)) return View();
                var add = await UserManager.AddPasswordAsync(user.Id, vm.Password);
                if (WasSuccessful(add)) return View("Index");
            }
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost, Route("DeleteUser")]
        public async Task<ActionResult> DeleteUser(DeleteUserViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(vm.Username);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found");
                    return View();
                }
                var delete = await UserManager.DeleteAsync(user);
                if (WasSuccessful(delete)) return View("Index");
            }
            return View();
        }


        private bool WasSuccessful(IdentityResult result)
        {
            if (result.Succeeded) return true;
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
            return false;
        }
    }
}