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
                var poll = await GetPoll(dbContext, id);
                return poll != null
                    ? new LunchPollViewModel(poll)
                    : null;
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

        [Route("{name}")]
        [HttpPost]
        [ApiForbid(Roles = "Goon")]
        public async Task<IHttpActionResult> CreatePoll(string name)
        {
            using (var dbContext = new DatabaseContext())
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest().WithReason("A name is required");

                var exists = await GetPolls(dbContext, DateTime.Now)
                    .AnyAsync(p => p.Name == name);
                if (exists)
                    return StatusCode(HttpStatusCode.Conflict)
                        .WithReason("A poll with the same name already exists");

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
                return Ok();
            }
        }

        [Route("{id}/Join")]
        [HttpPut]
        [ApiForbid(Roles = "Goon")]
        public async Task<IHttpActionResult> JoinPoll(int id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var currentUser = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);

                var poll = await GetPoll(dbContext, id);
                if (poll == null) return NotFound().WithReason("No poll found with the given id.");

                await AddToPoll(dbContext, poll, currentUser);

                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }

        [Route("{id}/Leave")]
        [HttpPut]
        public async Task<IHttpActionResult> LeavePoll(int id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPoll(dbContext, id);
                if (poll == null) return NotFound().WithReason("No poll found with the given id.");

                var currentUser = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);
                RemoveFromPoll(dbContext, poll, currentUser);

                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }

        [Route("{id}/Vote")]
        [HttpPut]
        public async Task<IHttpActionResult> Vote([FromUri]int id, [FromBody]LunchVoteViewModel vote)
        {
            using (var dbContext = new DatabaseContext())
            {
                if (vote == null)
                    return StatusCode(HttpStatusCode.BadRequest).WithReason("What the fuck are you doing");
                if (vote.Score > 1 || vote.Score < -1)
                    return StatusCode(HttpStatusCode.Forbidden).WithReason("Stop fucking with the votes.");

                var poll = await GetPoll(dbContext, id);
                if (poll == null)
                    return NotFound().WithReason("No poll found with the given id.");
                if (poll.Date.Date != DateTime.Now.Date)
                    return StatusCode(HttpStatusCode.Forbidden).WithReason("Stop fucking with the past.");

                var user = await dbContext.Users.SingleAsync(u => u.UserName == User.Identity.Name);
                if (!poll.Voters.Contains(user))
                    return StatusCode(HttpStatusCode.Forbidden).WithReason("Only members are allowed to vote.");

                if (string.IsNullOrWhiteSpace(vote.Name))
                    return StatusCode(HttpStatusCode.BadRequest).WithReason("Lunch option not specified");
                var option = await GetOrAddOption(dbContext, vote.Name);


                var currentVote = poll.Votes.SingleOrDefault(v => v.User == user && v.Option == option);
                if ((currentVote?.Score ?? 0) == vote.Score)
                {
                    await dbContext.SaveChangesAsync();
                    return Ok();
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

                LunchHub.OnVote(poll.Id, option, poll.Votes.Where(v => v.Option == option));

                return Ok();
            }
        }

        [Route("{id}/Add/{username}")]
        [HttpPut]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<IHttpActionResult> AddToPoll(int id, string username)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPoll(dbContext, id);
                if (poll == null) return NotFound().WithReason("No poll found with the given id.");
                var user = await dbContext.Users.SingleOrDefaultAsync(p => p.UserName == username);
                if (user == null) return NotFound().WithReason("No user found with the given name.");

                await AddToPoll(dbContext, poll, user);

                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }

        [Route("{id}/Remove/{username}")]
        [HttpPut]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<IHttpActionResult> RemoveFromPoll(int id, string username)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPoll(dbContext, id);
                if (poll == null) return NotFound().WithReason("No poll found with the given id.");
                var user = await dbContext.Users.SingleOrDefaultAsync(p => p.UserName == username);
                if (user == null) return NotFound().WithReason("No user found with the given name.");

                RemoveFromPoll(dbContext, poll, user);

                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }

        [Route("{id}/Decide/{decision}")]
        [HttpPut]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<IHttpActionResult> Decide(int id, string decision)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPoll(dbContext, id);
                if (poll == null) return NotFound().WithReason("No poll found with the given id.");
                if (string.IsNullOrWhiteSpace(decision)) return BadRequest().WithReason("Lunch option not specified");
                poll.Decision = await GetOrAddOption(dbContext, decision);

                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }

        [Route("Options/{name}/Rename")]
        [HttpPut]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<IHttpActionResult> RenameOption(string name, string newName)
        {
            using (var dbContext = new DatabaseContext())
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(newName))
                    return BadRequest().WithReason("Lunch option not specified");
                var oldOption = await dbContext.LunchOptions.Where(o => o.Name == name).SingleOrDefaultAsync();
                if (oldOption == null)
                    return NotFound();

                var newOption = await dbContext.LunchOptions.SingleOrDefaultAsync(o => o.Name == newName);
                if (newOption == null)
                {
                    // easy, literally rename the old option
                    oldOption.Name = newName;
                }
                else
                {
                    // replace all references to the old one with references to the new
                    // this could mess with scores
                    foreach (var poll in GetPolls(dbContext).Where(p => p.Decision.Id == oldOption.Id))
                        poll.Decision = newOption;

                    var oldVotes = await dbContext.LunchVotes
                        .Include(v => v.Poll).Include(v => v.User)
                        .Where(v => v.Option.Id == oldOption.Id)
                        .ToListAsync();
                    var newVotes = await dbContext.LunchVotes
                        .Include(v => v.Poll).Include(v => v.User)
                        .Where(v => v.Option.Id == newOption.Id)
                        .ToDictionaryAsync(v => new { v.Poll, v.User });

                    var toAdd =
                        from old in oldVotes
                        where !newVotes.ContainsKey(new { old.Poll, old.User })
                        select new LunchVote
                        {
                            Poll = old.Poll,
                            User = old.User,
                            Option = newOption,
                            Score = old.Score
                        };

                    dbContext.LunchVotes.AddRange(toAdd);
                    dbContext.LunchVotes.RemoveRange(oldVotes);
                    dbContext.LunchOptions.Remove(oldOption);
                }


                // Refresh all of today's polls in case any scores have changed
                foreach (var poll in GetPolls(dbContext, DateTime.Now))
                    LunchHub.OnPollChanged(new LunchPollViewModel(poll));

                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }

        [Route("Options/{name}")]
        [HttpDelete]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<IHttpActionResult> DeleteOption(string name)
        {
            using (var dbContext = new DatabaseContext())
            {
                var option = await dbContext.LunchOptions.Where(o => o.Name == name).SingleOrDefaultAsync();
                if (option == null)
                    return NotFound();

                foreach (var poll in GetPolls(dbContext).Where(p => p.Decision.Id == option.Id))
                    poll.Decision = null;
                dbContext.LunchOptions.Remove(option);

                LunchHub.OnOptionDeleted(option.Id);

                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }

        [Route("{id}")]
        [HttpDelete]
        [ApiAuthorize(Roles = "Lunch Administrator")]
        public async Task<IHttpActionResult> DeletePoll(int id)
        {
            using (var dbContext = new DatabaseContext())
            {
                var poll = await GetPoll(dbContext, id);
                if (poll == null)
                    return NotFound();
                dbContext.LunchPolls.Remove(poll);

                LunchHub.OnPollDeleted(id);

                await dbContext.SaveChangesAsync();
                return Ok();
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

        public Task<LunchPoll> GetPoll(DatabaseContext dbContext, int id)
        {
            return GetPolls(dbContext).SingleOrDefaultAsync(p => p.Id == id);
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

    }
}
