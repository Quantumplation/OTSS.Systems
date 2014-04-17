using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using Website.Models;
using Website.ViewModels.API;

namespace Website.Controllers.API
{
    [RoutePrefix("api/Users")]
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

    }
}