using System.ComponentModel.DataAnnotations;

namespace Alloc8_web.ViewModels.Account
{
    public class UserChangePasswordViewModal
    {
        public string email { get; set; }
        public string token { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string password { get; set; }

        [Compare("password", ErrorMessage = "Passwords do not match")]
        public string reEnteredPassword { get; set; }
    }
}
