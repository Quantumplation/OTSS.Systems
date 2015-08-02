using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Website.Hubs;
using Website.Models;
using Website.ViewModels.API;

namespace Website.Controllers.API
{
    [RoutePrefix("api/Lunch")]
    [ApiAuthorize]
    public class LunchController : ApiController
    {
        [Route("Vote")]
        [HttpPut]
        public async Task<bool> Vote([FromBody]LunchVoteViewModel vote)
        {
            using (var dbContext = new DatabaseContext())
            {
                if (vote.Score > 1 || vote.Score < -1)
                    return false;

                var option = await dbContext.LunchOptions.SingleOrDefaultAsync(o => o.Name == vote.OptionName);
                var user = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);

                if (option == null)
                {
                    option = new LunchOption { Name = vote.OptionName };
                    dbContext.LunchOptions.Add(option);
                }

                var poll = await dbContext.LunchPolls
                    .Include(p => p.Votes.Select(v => v.User))
                    .Include(p => p.Votes.Select(v => v.Option))
                    .SingleOrDefaultAsync(p => p.Id == vote.PollId);

                var currentVote = poll.Votes.SingleOrDefault(v => v.User == user && v.Option == option);
                if (currentVote?.Score == vote.Score)
                {
                    await dbContext.SaveChangesAsync();
                    return true;
                }

                if (vote.Score == 0)
                {
                    dbContext.LunchVotes.Remove(currentVote);
                }
                else if (currentVote != null)
                {
                    currentVote.Score = vote.Score;
                }
                else
                {
                    dbContext.LunchVotes.Add(new LunchVote
                    {
                        Poll = poll,
                        Option = option,
                        User = user,
                        Score = vote.Score
                    });
                }
                await dbContext.SaveChangesAsync();

                LunchHub.OnVote(option, poll.Votes.Where(v => v.Option == option), user.UserName);

                return true;
            }
        }

        [Route("Options/{q?}")]
        public IEnumerable<LunchOption> GetOptions(string q = "")
        {
            using (var dbContext = new DatabaseContext())
            {
                return dbContext.LunchOptions.Where(o => o.Name.Contains(q)).ToList();
            }
        }
    }
}
