using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Website.Hubs;
using Website.Models;
using Website.ViewModels.Web;
using System.Data.Entity;
namespace Website.Controllers
{
    [RoutePrefix("Quotes")]
    [Authorize]
    public class QuotesController : Controller
    {
        [Route("")]
        [HttpGet]
        public ActionResult Index()
        {
            
            var vm = new QuoteListViewModel();
            using (var dbContext = new DatabaseContext())
            {
                vm.Quotes = dbContext.Quotes
                                     .Include(p => p.Author)
                                     .Include(p => p.Submitter)
                                     .Include(p => p.Tags).ToList()
                                     .Select(x => new QuoteViewModel(x)).ToList();
                vm.Tags = dbContext.Tags.ToList()
                                   .Select(t => new TagViewModel(t)).ToList();
            }
            return View(vm);
        }

        [Route("NewQuote")]
        [HttpPost]
        public async Task<ActionResult> NewQuote(QuoteViewModel vm)
        {
            using (var dbContext = new DatabaseContext())
            {
                var quote = new Quote
                {
                    Text = vm.Text,
                    Submitter = dbContext.Users.Single(u => u.UserName == User.Identity.Name),
                    CreatedAt = DateTime.Now,
                    Tags = new List<Tag>()
                };
                var user = dbContext.Users.SingleOrDefault(x => x.UserName == vm.Author);
                if (user != null)
                {
                    quote.Author = user;
                }
                else
                {
                    quote.AlternateAuthor = String.IsNullOrWhiteSpace(vm.Author) ? "Anonymous" : vm.Author;
                }
                dbContext.Quotes.Add(quote);
                vm = new QuoteViewModel(quote);
                await dbContext.SaveChangesAsync();
                QuotesHub.NewQuote(quote);
            }
            return PartialView("_Quote", vm);
        }

        [HttpGet]
        [Route("SendSpecificToBanter")]
        public ActionResult SendSpecificToBanter(int Id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var quote =
                    dbContext.Quotes.Include(x => x.Author).Include(x => x.Submitter).Single(x => x.Id == Id);
                QuotesHub.NewQuote(quote);
            }
            return RedirectToAction("Index");
        }
    }
}