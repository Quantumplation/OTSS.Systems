using System.Web.Mvc;

namespace Website.Controllers
{
    [RoutePrefix("")]
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [Route("SignalRSample")]
        public ActionResult SignalRSample()
        {
            return View();
        }
    }
}
