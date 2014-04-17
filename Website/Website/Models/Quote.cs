using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Context { get; set; }
        [InverseProperty("AuthoredQuotes")]
        public User Author { get; set; }
        public string AlternateAuthor { get; set; }

        [InverseProperty("SubmittedQuotes")]
        public User Submitter { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public virtual ICollection<Tag> Tags { get; set; } 
    }
}