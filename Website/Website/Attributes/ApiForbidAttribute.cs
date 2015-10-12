using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Website
{
    public class ApiForbidAttribute : ApiAuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var user = actionContext.RequestContext.Principal;
            return !Roles.Split(',').Any(user.IsInRole);
        }
    }
}