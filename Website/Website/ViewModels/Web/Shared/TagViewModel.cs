using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Models;

namespace Website.ViewModels.Web
{
    public class TagViewModel
    {
        public TagViewModel()
        {
            
        }

        public TagViewModel(Tag tag)
        {
            Text = tag.Text;
        }

        public string Text { get; set; }
    }
}