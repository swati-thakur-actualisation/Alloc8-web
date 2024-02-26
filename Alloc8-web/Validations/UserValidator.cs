using Alloc8_web.Utilities;
using Alloc8_web.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Alloc8_web.Validations
{
    public class UserValidator
    {
        public static bool CanAddorEdit(ClaimsPrincipal user,string role)
        {
           
            if (!WebSiteRoles.getRoles().Any(x=>x==role))
            {
                throw new Exception("Invalid Role");
            }
            if (user.IsInRole(WebSiteRoles.Website_User) && (role == WebSiteRoles.Website_Manager || role == WebSiteRoles.Website_Admin))
            {
                // user can only add users
                throw new Exception("You don't have permission to assign admin or manager role");
            }
            if (user.IsInRole(WebSiteRoles.Website_Manager) && (role == WebSiteRoles.Website_Manager || role == WebSiteRoles.Website_Admin))
            {
                // manager can only add users
                throw new Exception("You don't have permission to assign admin or manager role");
            }
            return true;
        }
        
    }
}
