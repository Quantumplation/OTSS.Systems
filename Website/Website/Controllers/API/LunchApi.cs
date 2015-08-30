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
        [Route("{date:datetime:regex(\\d{4}-\\d{2}-\\d{2})}/")]
        public async Task<IEnumerable<LunchPollViewModel>> GetPolls(DateTime date)
        {
            using (var dbContext = new DatabaseContext())
            {
                var polls = await GetPolls(dbContext, date).ToListAsync();
                return polls.Select(p => new LunchPollViewModel(p, User.Identity.Name)).ToList();
            }
        }

        [Route("{date:datetime:regex(\\d{4}-\\d{2}-\\d{2})}/{name}")]
        public async Task<LunchPollViewModel> GetPoll(DateTime date, string name)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPolls(dbContext, date).SingleOrDefaultAsync(p => p.Name == name);
                return poll != null
                    ? new LunchPollViewModel(poll, User.Identity.Name)
                    : null;
            }
        }

        public IQueryable<LunchPoll> GetPolls(DatabaseContext dbContext)
        {
            return dbContext.LunchPolls
                .Include(p => p.Voters)
                .Include(p => p.Votes.Select(v => v.User))
                .Include(p => p.Votes.Select(v => v.Option));
        }

        public IQueryable<LunchPoll> GetPolls(DatabaseContext dbContext, DateTime date)
        {
            date = date.Date;
            return GetPolls(dbContext).Where(p => DbFunctions.TruncateTime(p.Date) == date);
        }

        [Route("{name}")]
        [HttpPost]
        public async Task<bool> CreatePoll(string name)
        {
            using (var dbContext = new DatabaseContext())
            {
                var exists = await GetPolls(dbContext, DateTime.Now)
                    .AnyAsync(p => p.Name == name);

                if (exists) return false;

                var currentUser = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);
                var poll = new LunchPoll
                {
                    Name = name,
                    Date = DateTime.Now,
                    Voters = new List<User> { currentUser },
                    Votes = new List<LunchVote>()
                };
                dbContext.LunchPolls.Add(poll);
                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        [Route("Vote")]
        [HttpPut]
        public async Task<bool> Vote([FromBody]LunchVoteViewModel vote)
        {
            using (var dbContext = new DatabaseContext())
            {
                if (vote.Score > 1 || vote.Score < -1)
                    return false;

                var user = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);
                var option = await GetOrAddOption(dbContext, vote.OptionName);

                var poll = await GetPolls(dbContext).SingleAsync(p => p.Id == vote.PollId);

                var currentVote = poll.Votes.SingleOrDefault(v => v.User == user && v.Option == option);
                if ((currentVote?.Score ?? 0) == vote.Score)
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
                if (!poll.Voters.Contains(user))
                    poll.Voters.Add(user);
                await dbContext.SaveChangesAsync();

                LunchHub.OnVote(option, poll.Votes.Where(v => v.Option == option));

                return true;
            }
        }

        public async Task<LunchOption> GetOrAddOption(DatabaseContext dbContext, string name)
        {
            var option = await dbContext.LunchOptions.SingleOrDefaultAsync(o => o.Name == name);
            if (option == null)
            {
                option = new LunchOption { Name = name };
                dbContext.LunchOptions.Add(option);
            }
            return option;
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
