using System.Collections.Generic;

namespace Website.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; } 
    }
}