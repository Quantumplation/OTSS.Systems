using System.ComponentModel.DataAnnotations;
using Website.Models;

namespace Website.ViewModels.Web
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