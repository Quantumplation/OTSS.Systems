using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Context { get; set; }
        public User Author { get; set; }
        public string AlternateAuthor { get; set; }
        public User Submitter { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<Tag> Tags { get; set; } 
    }
}