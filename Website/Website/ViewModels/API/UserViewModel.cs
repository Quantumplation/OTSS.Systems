using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Models;

namespace Website.ViewModels.API
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            
        }

        public UserViewModel(User u)
        {
            Id = u.Id;
            UserName = u.UserName;
            AuthoredQuoteIds = u.AuthoredQuotes.Select(x => x.Id);
            SubmittedQuoteIds = u.SubmittedQuotes.Select(x => x.Id);
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public IEnumerable<int> AuthoredQuoteIds { get; set; }
        public IEnumerable<int> SubmittedQuoteIds { get; set; } 
    }
}