using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Website.ViewModels.Web
{
    public class NewLunchPollViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}