using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Alloc8_web.Utilities
{
    public class DbInitializer : IDbInitializer
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private Alloc8DbContext _context;
        private SignInManager<ApplicationUser> _signInManager;
        public DbInitializer(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            Alloc8DbContext context
            )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public void Initialize()
        {
            try
            {
                var migrations = _context.Database.GetPendingMigrations();
                if (migrations.Count() >0)
                {
                     _context.Database.Migrate();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initalizing Db {ex.Message}");
            }
            if (!_roleManager.RoleExistsAsync(WebSiteRoles.Website_Admin).GetAwaiter().GetResult())
            {
                 _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Admin)).GetAwaiter().GetResult();
                 _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Manager)).GetAwaiter().GetResult();
                 _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_User)).GetAwaiter().GetResult();

                // Create the admin user
                var adminUser = new IdentityUser
                {
                    UserName = "aaron@actualisation.ai", // Change the username as needed
                    Email = "aaron@actualisation.ai", // Change the email as needed
                };
                string adminPassword = "yRl78t6ftbiujv6uQ3kg41sMsQj"; // Change the password as needed
                var applicationUser = _context.users.Where(x => x.Email == adminUser.Email).FirstOrDefault();


                if (applicationUser == null)
                {
                    ApplicationUser user = new ApplicationUser();
                    user.Email = adminUser.Email;
                    user.UserName = adminUser.UserName;
                    user.isActive = true;
                    user.firstName = "Aaron";
                    user.lastName = "Zamykal";
                    user.PasswordHash = Helper.HashPassword(adminPassword);
                    user.is_deleted = false;
                    user.roleName = WebSiteRoles.Website_Admin;
                    _context.Add(user);
                    var result = _context.SaveChanges();
                    //var createPowerUser = _userManager.CreateAsync(user, adminPassword).GetAwaiter().GetResult();
                    
                    // Assign the admin role to the user
                    _signInManager.RefreshSignInAsync(user);
                     _userManager.AddToRoleAsync(user, WebSiteRoles.Website_Admin).GetAwaiter().GetResult();
                    // applyingPermissions
                    AddSuperAdminPermissions(user);
                    
                }
            }
        }
        public void AddSuperAdminPermissions(ApplicationUser user)
        {

            List<UserPermission> permissions = Helper.getPermissions();
            foreach(UserPermission permission in permissions)
            {
                permission.view = true;
                permission.edit = true;
                permission.remove = true;
            }
            //permissions.Add(new UserPermission
            //{
            //    name = "dashboard",
            //    view = true,
            //    edit = true,
            //    remove = true,
            //    user = user
            //});
            //permissions.Add(new UserPermission
            //{
            //    name = "users",
            //    view = true,
            //    edit = true,
            //    remove = true,
            //    user = user
            //});
            ////permissions.Add(new UserPermission
            ////{
            ////    name = "devices",
            ////    view = true,
            ////    edit = true,
            ////    remove = true,
            ////    user = user
            ////});
            ////permissions.Add(new UserPermission
            ////{
            ////    name = "chat",
            ////    view = true,
            ////    edit = true,
            ////    remove = true,
            ////    user = user
            ////});
            //permissions.Add(new UserPermission
            //{
            //    name = "apps",
            //    view = true,
            //    edit = true,
            //    remove = true,
            //    user = user
            //});
            //permissions.Add(new UserPermission
            //{
            //    name = "chatgpt",
            //    view = true,
            //    edit = true,
            //    remove = true,
            //    user = user
            //});
            var claims = new List<Claim>();

            foreach (var permission in permissions)
            {
                permission.user = user;
                if (permission.view)
                {
                    claims.Add(new Claim("permission", $"{permission.name}_view"));
                }
                if (permission.edit)
                {
                    claims.Add(new Claim("permission", $"{permission.name}_edit"));
                }
                if (permission.remove)
                {
                    claims.Add(new Claim("permission", $"{permission.name}_remove"));
                }
            }
            _userManager.AddClaimsAsync(user,claims).GetAwaiter().GetResult();
            _context.AddRange(permissions);
            _context.SaveChanges();
            
        }
    }
}
