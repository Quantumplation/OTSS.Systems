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
            Text = quote.Text;
            Author = quote.Author != null ? quote.Author.UserName : quote.AlternateAuthor ?? "Anonymous";
            Tags = quote.Tags.Select(tag => new TagViewModel(tag)).ToList();
        }

        public string Text { get; set; }
        public string Author { get; set; }

        public ICollection<TagViewModel> Tags { get; set; } 
    }
}