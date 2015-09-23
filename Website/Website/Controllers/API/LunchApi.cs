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
        [Route("{date:datetime:regex(\\d{4}-\\d{2}-\\d{2})}")]
        public async Task<IEnumerable<LunchPollViewModel>> GetPolls(DateTime date)
        {
            using (var dbContext = new DatabaseContext())
            {
                var polls = await GetPolls(dbContext, date).ToListAsync();
                return polls.Select(p => new LunchPollViewModel(p)).ToList();
            }
        }

        [Route("{id:int}")]
        public async Task<LunchPollViewModel> GetPoll(int id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPolls(dbContext).SingleOrDefaultAsync(p => p.Id == id);
                return poll != null
                    ? new LunchPollViewModel(poll)
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
                    Voters = new List<User>(),
                    Votes = new List<LunchVote>()
                };
                dbContext.LunchPolls.Add(poll);
                await AddToPoll(dbContext, poll, currentUser);
                await dbContext.SaveChangesAsync();

                LunchHub.OnPollChanged(new LunchPollViewModel(poll));
                return true;
            }
        }

        [Route("{id}/Join")]
        [HttpPut]
        public async Task<bool> JoinPoll(int id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var currentUser = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);

                var poll = await GetPolls(dbContext).SingleOrDefaultAsync(p => p.Id == id);
                if (poll == null) return false;

                await AddToPoll(dbContext, poll, currentUser);

                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        [Route("{id}/Leave")]
        [HttpPut]
        public async Task<bool> LeavePoll(int id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPolls(dbContext).SingleOrDefaultAsync(p => p.Id == id);
                if (poll == null) return false;

                var currentUser = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);
                RemoveFromPoll(dbContext, poll, currentUser);

                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        [Route("{id}/Remove/{username}")]
        [HttpPut]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<bool> RemoveFromPoll(int id, string username)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPolls(dbContext).SingleOrDefaultAsync(p => p.Id == id);
                if (poll == null) return false;
                var user = await dbContext.Users.SingleOrDefaultAsync(p => p.UserName == username);
                if (user == null) return false;

                RemoveFromPoll(dbContext, poll, user);

                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        [Route("{id}/Decide/{decision}")]
        [HttpPut]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<bool> Decide(int id, string decision)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPolls(dbContext).SingleOrDefaultAsync(p => p.Id == id);
                if (poll == null) return false;
                poll.Decision = await GetOrAddOption(dbContext, decision);

                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task AddToPoll(DatabaseContext dbContext, LunchPoll poll, User user)
        {
            var existingPolls = await GetPolls(dbContext, DateTime.Now)
                .Where(p => p.Voters.Select(v => v.Id).Contains(user.Id))
                .ToListAsync();
            if (existingPolls.Contains(poll))
                return;

            foreach (var existingPoll in existingPolls)
                RemoveFromPoll(dbContext, existingPoll, user);
            poll.Voters.Add(user);

            LunchHub.OnPollChanged(new LunchPollViewModel(poll));
        }

        public void RemoveFromPoll(DatabaseContext dbContext, LunchPoll poll, User user)
        {
            poll.Voters.Remove(user);
            var userVotes = poll.Votes.Where(v => v.User == user).ToList();
            foreach (var vote in userVotes)
            {
                var remainingVotes = poll.Votes
                    .Where(v => v.Option == vote.Option)
                    .Except(new[] { vote });
                LunchHub.OnVote(poll.Id, vote.Option, remainingVotes);
                dbContext.LunchVotes.Remove(vote);
            }
            LunchHub.OnPollChanged(new LunchPollViewModel(poll));
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

                LunchHub.OnVote(poll.Id, option, poll.Votes.Where(v => v.Option == option));

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
