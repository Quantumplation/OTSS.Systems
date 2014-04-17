using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; } 
    }
}