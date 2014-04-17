using System.Collections.Generic;
using Website.Models;

namespace Website.ViewModels.Quotes
{
    public class QuoteListViewModel
    {
        public ICollection<QuoteViewModel> Quotes { get; set; } 
        public ICollection<TagViewModel> Tags { get; set; } 
    }
}