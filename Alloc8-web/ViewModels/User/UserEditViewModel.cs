using Alloc8.ef.Entities.Dashboard;
using Alloc8_web.Utilities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Alloc8_web.ViewModels.User
{
    public class UserEditViewModel
    {
        [Required(ErrorMessage = "User is required")]
        public string? userId { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        public string? firstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? lastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string? password { get; set; }

        [Compare("password", ErrorMessage = "Passwords do not match")]
        public string? reEnteredPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string? role { get; set; }

        // Other properties

        public bool isActive { get; set; }
        //public List<string>? managers { get; set; }
        public List<string?>? managers { get; set; }
        public PagedResult<UserViewModel>? allManagers { get; set; }
        public List<Alloc8.ef.Entities.Dashboard.Organisation>? organisations { get; set; }
        
        public int? orginationId { get; set; }

        public List<Permissions>? permissions { get; set; }
        public string timezone { set; get; }

    }
    public class UserOrganisationAndManager
    {
        public PagedResult<UserViewModel>? allManagers { get; set; }
        public List<Alloc8.ef.Entities.Dashboard.Organisation> organisations { get; set; }

    }


}
