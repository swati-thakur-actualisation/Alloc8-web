using Alloc8_web.Filters;
using Alloc8_web.Services.Azure;
using Alloc8_web.Services.User;
using Alloc8_web.Utilities;
using Alloc8_web.Validations;
using Alloc8_web.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using Alloc8.ef.Entities.Dashboard;
using Alloc8.ef;

namespace Alloc8_web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private Alloc8DbContext _context;
        private UserManager<Alloc8.ef.Entities.Dashboard.ApplicationUser> _userManager;
        private IUserService _userService;
        private IAuth _userPremissionService;
        private IAzureBlobService _azureBlob;
        private string _profilePhotosContainer = "images";
        public UserController(Alloc8DbContext context, UserManager<Alloc8.ef.Entities.Dashboard.ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<Alloc8.ef.Entities.Dashboard.ApplicationUser> signInManager, IUserService userService, IAuth userPremissionService,IAzureBlobService azureBlobService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
            _userPremissionService = userPremissionService;
            _azureBlob = azureBlobService;
        }
        [AuthorizePermission("users", "view")]

        public async Task<IActionResult> Index()
        {

            if (User.IsInRole(WebSiteRoles.Website_Admin))
            {
                return View(await _userService.getAllUsers());
            }
            if (User.IsInRole(WebSiteRoles.Website_Manager))
            {
                Alloc8.ef.Entities.Dashboard.ApplicationUser? currentLoginUser = await _userManager.GetUserAsync(User);
                if (currentLoginUser == null)
                {
                    return View();
                }
                return View(await _userService.getUserForManager(currentLoginUser.Id));

            }

            return View(new PagedResult<List<UserViewModel>>
            {
                data = null
            });

        }
        [AuthorizePermission("users", "view")]

        [HttpGet]
        public async Task<IActionResult> GetUsersTable(int pageNumber = 1, string search = "")
        {
            if (User.IsInRole(WebSiteRoles.Website_Admin))
            {
                return PartialView(await _userService.getAllUsers(pageNumber, 10, search));
            }
            if (User.IsInRole(WebSiteRoles.Website_Manager))

            {
                ApplicationUser? currentLoginUser = await _userManager.GetUserAsync(User);
                if (currentLoginUser == null)
                {
                    return PartialView();
                }

                return PartialView(_userService.getUserForManager(currentLoginUser.Id, pageNumber, 10, search));

            }

            return View(new PagedResult<List<UserViewModel>>
            {
                data = null
            });
        }
        [AuthorizePermission("users", "edit")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //datbase context
            var filterdata = new UserOrganisationAndManager();

            filterdata.organisations = _context.organisations.Where(x => x.isDeleted == false).ToList();
            filterdata.allManagers = await _userService.getAllManagers();           

            return View( filterdata );

            

        }
        [AuthorizePermission("users", "edit")]
        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ModelStateValidator.throwValidationErrors(this);
            }

            UserValidator.CanAddorEdit(User, model.role);

            try
            {
                // check if user exists
                if(await _context.users.Where(x=>x.Email == model.email).AnyAsync())
                {
                    return BadRequest(new {status = 0, errors = new {email="This Email is already in use"}});
                }
                ApplicationUser newUser = new ApplicationUser();
                newUser.Email = model.email;
                newUser.firstName = model.firstName;
                newUser.lastName = model.lastName;
                newUser.PasswordHash = Helper.HashPassword(model.password);
                newUser.roleName = model.role;
                newUser.UserName = model.email;
                newUser.isActive = model.isActive;
                newUser.orginationId = model.orginationId;
                newUser.timezone = model.timezone;

                IdentityResult result = await _userManager.CreateAsync(newUser, model.password);

                if (!result.Succeeded)
                {
                    var identityErrors = result.Errors
                        .GroupBy(e => e.Code) // Grouping by error code, alternatively you can use a custom key
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(e => e.Description).ToArray()
                        );

                    return BadRequest(new { message = "User creation failed", status = 0, errors = identityErrors });
                }

                // assiging role

                await _userManager.AddToRoleAsync(newUser, model.role);


                if (model.permissions != null && model.permissions.Any())
                {
                    foreach (var permission in model.permissions)
                    {
                        _context.Add(new UserPermission
                        {
                            name = permission.name,
                            view = permission.view,
                            edit = permission.edit,
                            remove = permission.remove,
                            user = newUser
                        });
                    }
                }
                if (model.managers != null && model.managers.Any())
                {
                    foreach (string managerId in model.managers)
                    {
                        ApplicationUser? manager = await _context.users.Where(x => x.Id == managerId).FirstOrDefaultAsync();
                        if (manager == null)
                        {
                            continue;
                        }
                        UserManager userManager = new UserManager();
                        userManager.manager = manager;
                        userManager.user = newUser;
                        _context.Add(userManager);

                    }
                }
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ObjectResult(new { message = ex.Message, status = 0, error = new { } }) { 
                    StatusCode = 500
                };
            }
            return Ok(new { status = 1 });
        }
        [AuthorizePermission("users", "edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }   

            ApplicationUser? user = await _context.users.Include(i => i.permissions).Include(x => x.managers).Where(x => x.Id == id).FirstOrDefaultAsync(); 
            if (user == null)
            {
                return BadRequest();
            }
           
            var userRoles = await _userManager.GetRolesAsync(user);

            var permissions = Helper.getPermissions();
            var userPermissions = new List<Permissions>();
            foreach (var permission in permissions)
            {
                var userPermission = new Permissions
                {
                    view = permission.view,
                    edit = permission.edit,
                    remove = permission.remove,
                    name = permission.name
                };
                var userPerm = user.permissions?.Where(x => x.name == permission.name).FirstOrDefault();
                if (userPerm == null)
                {
                    userPermissions.Add(userPermission);
                    continue;
                }

                userPermission.view = userPerm.view;
                userPermission.edit = userPerm.edit;
                userPermission.remove = userPerm.remove;
                userPermission.name = userPerm.name;
                userPermissions.Add(userPermission);
            }
            var allMangersList = await _userService.getAllManagers();
            

            var model = new UserEditViewModel
            {
                userId = user.Id,
                firstName = user.firstName,
                lastName = user.lastName,
                email = user.Email,
                timezone=user.timezone,
                isActive = user.isActive,
                role = userRoles.FirstOrDefault(), // Assuming a user has one role
                permissions = userPermissions,
                managers = user?.managers?.Select(x => x.managerId).ToList(),
                allManagers = allMangersList,
                organisations= _context.organisations.Where(x => x.isDeleted == false).ToList(),
                orginationId = user?.orginationId,
            };
            return View(model);
        }
        [AuthorizePermission("users", "edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                return ModelStateValidator.throwValidationErrors(this);
            }
            if(model.role == null)
            {
                return  BadRequest(new {message ="Select correct role",status = 0, errors = new {role="incorrect role"}});
            }
            var user = await _context.users.Include(u => u.managers).Include(u => u.permissions).Where(x => x.Id == model.userId).FirstOrDefaultAsync(); ;
            if (user == null)
            {
                return NotFound();
            }

            UserValidator.CanAddorEdit(User, model.role);
            try
            {
                user.Email = model.email;
                user.firstName = model.firstName;
                user.lastName = model.lastName;
                user.isActive = model.isActive;
                user.roleName = model.role;
                user.timezone = model.timezone;
                var organisation = await _context.organisations.Where(x => x.Id == model.orginationId).FirstOrDefaultAsync();
                if(organisation != null)
                {
                    user.orgination = organisation;
                }

                // Update the user's password if it's been changed
                if (!string.IsNullOrEmpty(model.password))
                {
                    if (model.password != model.reEnteredPassword)
                    {
                        return BadRequest(new { message = "Validation failed", status = 0, errors = new { reEnteredPassword = "Passwords do not match" } });
                    }
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.password);
                }

                var oldRole = await _userManager.GetRolesAsync(user);
                if (oldRole.FirstOrDefault() != model.role)
                {
                    if (oldRole.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, oldRole);
                    }
                    await _userManager.AddToRoleAsync(user, model.role);
                }
                if(model.permissions != null)
                {
                    foreach (var permission in model.permissions)
                    {
                        UserPermission? currentPermission = user?.permissions?.Where(x => x.name == permission.name).FirstOrDefault();
                        if (currentPermission != null)
                        {
                            currentPermission.remove = permission.remove;
                            currentPermission.edit = permission.edit;
                            currentPermission.view = permission.view;
                            _context.Entry(currentPermission).State = EntityState.Modified;
                        }
                        else
                        {
                            UserPermission newPermission = new UserPermission();
                            newPermission.name = permission.name;
                            newPermission.view = permission.view;
                            newPermission.edit = permission.edit;
                            newPermission.remove = permission.remove;
                            if (user != null)
                            {
                                newPermission.user = user;
                            }
                            _context.Add(newPermission);
                        }
                    }
                }
                var currentManagers = user?.managers?.ToList();

                if(currentManagers != null)
                {
                    if (model.managers == null )
                    {
                        foreach (var currentManager in currentManagers)
                        {
                            _context.Remove(currentManager);
                        }
                    }
                    else
                    {
                        foreach (var currentManager in currentManagers)
                        {
                            if (!model.managers.Any(m => m == currentManager.managerId))
                            {
                                _context.Remove(currentManager);
                            }
                        }
                        foreach (var newManager in model.managers)
                        {
                            if (!currentManagers.Any(cm => cm.managerId == newManager))
                            {
                                var managerEntity = await _context.users.FindAsync(newManager);
                                if (managerEntity != null)
                                {
                                    UserManager userManager = new UserManager
                                    {
                                        manager = managerEntity,
                                        user = user
                                    };
                                    _context.Add(userManager);
                                }
                            }
                        }

                    }
                }

                if(user == null)
                {
                    return BadRequest(new { message = "Error in saving user", status = 0, errors = new { } }) ;
                }
                await _context.SaveChangesAsync();
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ObjectResult(new { message = ex.Message, status = 0, error = new { } })
                {
                    StatusCode = 500
                };
            }
            
            return Ok(new { status = 1 });
        }
        [AuthorizePermission("users", "remove")]
        [HttpGet]
        public async Task<IActionResult> DoDelete(string id)
        {
            if (!_userPremissionService.hasPermission("users","remove"))
            {
                return NotFound();
            }
            var user = await _context.users.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return PartialView(new UserDeleteViewModel { userId = user.Id, email = user.Email });
        }
        [AuthorizePermission("users", "remove")]
        [HttpPost]
        public async Task<IActionResult> Delete(UserDeleteViewModel model)
        {
            if (!_userPremissionService.hasPermission("users", "remove"))
            {
                return BadRequest(new { status = 0 });
            }
            var user = await _context.users.Where(x => x.Id == model.userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest(new { status = 0 });
            }
            user.is_deleted = true;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = 1 });
        }
        [HttpGet]
        public async Task<IActionResult> Profile(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser? user = await _context.users.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest();
            }
            // getting permissions

            // Get user's roles and claims
            var userRoles = await _userManager.GetRolesAsync(user);


            var model = new UserViewModel
            {
                userId = user.Id,
                firstName = user.firstName,
                lastName = user.lastName,
                email = user.Email,
                active = user.isActive,
                profilePicture =user.profileImage,
                role = userRoles.FirstOrDefault(),
                timezone=user.timezone
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> uploadProfileImage(IFormFile profileImage)
        {
            if (profileImage == null)
            {
                return BadRequest();
            }
            try
            {
                string profileImageUrl = await _azureBlob.UploadFileAsync(profileImage, _profilePhotosContainer);
                if(!string.IsNullOrEmpty(profileImageUrl)) 
                {
                    var user = await _userManager.GetUserAsync(User);
                    if(user != null)
                    {
                        user.profileImage = profileImageUrl;
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }

                }
                return Ok(new { profileImageUrl = profileImageUrl });

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public async Task<IActionResult> DoLoginAs(string id)
        {
            var user = await _context.users.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return PartialView(new UserLoginAsViewModel { userId = user.Id, email = user.Email });

        }

        [HttpPost]
        public async Task<IActionResult> UpdateTimezone(string timezone, string userId)
        {
            try
            {
                var user = await _context.users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    user.timezone = timezone;
                    await _context.SaveChangesAsync();
                    return Json(new { status = 1, message = "Timezone updated successfully" });
                }
                return Json(new { status = 0, message = "User not found" });
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }

    }
}
