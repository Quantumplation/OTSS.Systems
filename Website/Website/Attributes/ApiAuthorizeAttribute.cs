using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Website
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "This method is forbidden, or you are not authenticated in.");
        }
    }
}