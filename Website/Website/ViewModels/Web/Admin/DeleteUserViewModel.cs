using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Website.ViewModels.Web
{
    public class DeleteUserViewModel
    {
        [Required]
        public string Username { get; set; }
    }
}