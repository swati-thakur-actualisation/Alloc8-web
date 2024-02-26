using Alloc8_web.ViewModels.User;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;

namespace Alloc8_web.Services.User
{
    public class Auth : IAuth
    {
        protected List<Alloc8.ef.Entities.Dashboard.UserPermission>? _permissions { get; set; }
        protected Alloc8.ef.Entities.Dashboard.ApplicationUser? _user { get; set; }   
        protected Alloc8DbContext _context { get; set; }
        protected IHttpContextAccessor _httpContextAccessor { get; set; }
        public Auth(Alloc8DbContext context, IHttpContextAccessor httpContextAccessor) 
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            string? email = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                _user = _context.users.Include(x => x.permissions).Where(x => x.Email == email).FirstOrDefault();
            }
            if(_user != null) 
            {
                _permissions = _user.permissions?.ToList();
            }
            
        }
        public bool hasPermission(string permissonName, string permissionType)
        {
            try
            {
                if(_permissions == null || !_permissions.Any()) 
                {
                    return false;
                }
                switch (permissionType)
                {
                    case "view":
                        return _permissions.Where(x=>x.name == permissonName && x.view == true).Any();
                    case "edit":
                        return _permissions.Where(x => x.name == permissonName && x.edit == true).Any();
                    case "remove":
                        return _permissions.Where(x => x.name == permissonName && x.remove == true).Any();
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        public UserLoginDataViewModel? user()
        {
            if (_user == null)
            {
                return null;
            }
            return new UserLoginDataViewModel
            {
                email = _user.Email,
                firstName = _user.firstName,
                lastName = _user.lastName,
                roleName = _user.roleName,
                fullName = _user.firstName + " " + _user.lastName,
                profileImage = _user.profileImage,
                timeZone = _user.timezone,
            };
        }
    }
}
