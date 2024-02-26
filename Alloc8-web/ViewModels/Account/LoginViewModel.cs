using System.ComponentModel.DataAnnotations;

namespace Alloc8_web.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Username")]
        public string email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

        [Display(Name = "Remember me")]
        public bool rememberMe { get; set; } = true;
    }
}
