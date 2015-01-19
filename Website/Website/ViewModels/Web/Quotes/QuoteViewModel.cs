using System;
using System.Collections.Generic;
using System.Linq;
using Website.Models;

namespace Website.ViewModels.Web
{
    public class QuoteViewModel
    {
        public QuoteViewModel()
        {
            
        }

        public QuoteViewModel(Quote quote)
        {
            Id = quote.Id;
            Text = quote.Text;
            Author = quote.Author != null ? quote.Author.UserName : quote.AlternateAuthor ?? "Anonymous";
            Tags = quote.Tags.Select(tag => new TagViewModel(tag)).ToList();
            CreatedAt = quote.CreatedAt;
            Submitter = quote.Submitter.UserName;
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        public string Submitter { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<TagViewModel> Tags { get; set; } 
    }
}