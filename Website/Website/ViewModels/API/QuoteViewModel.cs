using System;
using System.Collections.Generic;
using System.Linq;
using Website.Models;

namespace Website.ViewModels.API
{
    public class QuoteViewModel
    {
        public QuoteViewModel()
        {
            
        }

        public QuoteViewModel(Quote q)
        {
            Id = q.Id;
            Text = q.Text;
            Context = q.Context;
            AuthorId = q.Author == null ? null : q.Author.Id;
            SubmitterId = q.Submitter == null ? null : q.Submitter.Id;

            AlternateAuthor = q.AlternateAuthor;
            CreatedAt = q.CreatedAt;
            Tags = q.Tags.Select(x => x.Text).ToList();
        }

        public Quote ToQuote(DatabaseContext context)
        {
            var quote = new Quote();
            if (Id != 0)
                quote = context.Quotes.Find(quote.Id);
            else
                quote.CreatedAt = DateTime.Now;
            quote.Text = Text;
            quote.Context = Context;

            if(AuthorId != null && !context.Users.Any(u => u.Id == AuthorId))
                throw new InvalidOperationException();
            if(SubmitterId != null && !context.Users.Any(u => u.Id == SubmitterId))
                throw new InvalidOperationException();

            User author = null;
            User submitter = null;
            if (AuthorId != null)
                author = context.Users.Find(AuthorId);
            if (SubmitterId != null)
                submitter = context.Users.Find(SubmitterId);
            quote.Submitter = submitter;
            quote.Author = author;
            quote.AlternateAuthor = AlternateAuthor;

            quote.Tags = context.Tags.Where(x => Tags.Contains(x.Text)).ToList();
            return quote;
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public string Context { get; set; }
        public string AuthorId { get; set; }
        public string SubmitterId { get; set; }
        public string AlternateAuthor { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<string> Tags { get; set; }
    }
}