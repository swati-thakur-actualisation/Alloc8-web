using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace Alloc8_web.ViewModels.User
{
    public class UserCreateViewModel
    {
        public string? userId { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        public string firstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string password { get; set; }

        [Compare("password", ErrorMessage = "Passwords do not match")]
        public string reEnteredPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string role { get; set; }

        // Other properties

        public bool isActive { get; set; }
        public List<string>? managers {get;set;}

        public int? orginationId { get; set; }
        public string timezone { set; get; }
        public List<Permissions> permissions { get; set; }

    }
    public class Permissions
    {
        public string name { get; set; }
        public bool view { get; set; }
        public bool edit { get; set; }
        public bool remove { get; set; }
    }
    public enum Role
    {
        Admin,
        Manager,
        User
    }
}
