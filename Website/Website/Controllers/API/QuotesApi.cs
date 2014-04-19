using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Website.Models;
using System.Data.Entity;
using Website.ViewModels.API;

namespace Website.Controllers.API
{
    [RoutePrefix("api/Quotes")]
    [ApiAuthorize]
    public class QuotesController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IEnumerable<QuoteViewModel> Get()
        {
            using (var dbContext = new DatabaseContext())
            {
                return dbContext.Quotes
                                .Include(x => x.Author)
                                .Include(x => x.Submitter).ToList()
                                .Select(q => new QuoteViewModel(q)).ToList();
            }
        }

        [HttpGet]
        [Route("{id}")]
        public QuoteViewModel Get(int id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var quote = dbContext.Quotes
                                     .Include(x => x.Author)
                                     .Include(x => x.Submitter)
                                     .SingleOrDefault(q => q.Id == id);
                if (quote == null) return null;
                return new QuoteViewModel(quote);
            }
        }

        [HttpPut]
        [Route("")]
        public QuoteViewModel New([FromBody]QuoteViewModel q)
        {
            using (var dbContext = new DatabaseContext())
            {
                var quote = q.ToQuote(dbContext);
                dbContext.Quotes.Add(quote);
                dbContext.SaveChanges();
                q.Id = quote.Id;
            }
            return q;
        }
    }
}