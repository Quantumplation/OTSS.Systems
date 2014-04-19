using System.Collections.Generic;

namespace Website.ViewModels.Web
{
    public class QuoteListViewModel
    {
        public ICollection<QuoteViewModel> Quotes { get; set; } 
        public ICollection<TagViewModel> Tags { get; set; } 
    }
}