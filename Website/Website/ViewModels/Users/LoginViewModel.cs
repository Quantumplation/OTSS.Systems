using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Website.Models;

namespace Website.ViewModels
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
        }

        public LoginViewModel(User fromUser)
        {
            UserName = fromUser.UserName;
        }

        public User ToUser()
        {
            return new User
            {
                UserName = UserName,
            };
        }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}