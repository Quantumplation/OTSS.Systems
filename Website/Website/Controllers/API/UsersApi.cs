using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Website.Models;
using Website.ViewModels.API;
using Website.ViewModels.Web;

namespace Website.Controllers.API
{
    [RoutePrefix("api/Users")]
    [ApiAuthorize]
    public class UsersController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IEnumerable<UserViewModel> Get()
        {
            using (var dbContext = new DatabaseContext())
            {
                return dbContext.Users
                                .Include(x => x.AuthoredQuotes)
                                .Include(x => x.SubmittedQuotes).ToList()
                                .Select(u => new UserViewModel(u)).ToList();
            }
        }

        [HttpGet]
        [Route("{id}")]
        public UserViewModel Get(string id)
        {
            using (var dbContext = new DatabaseContext())
            {
                return dbContext.Users
                                .Include(x => x.AuthoredQuotes)
                                .Include(x => x.SubmittedQuotes)
                                .Where(x => x.Id == id).ToList()
                                .Select(u => new UserViewModel(u)).Single();
            }
        }
    }
}