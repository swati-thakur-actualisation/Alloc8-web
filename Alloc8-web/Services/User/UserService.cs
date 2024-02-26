using Alloc8_web.Utilities;
using Alloc8_web.ViewModels.User;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;

namespace Alloc8_web.Services.User
{
    public class UserService : IUserService
    {
        private Alloc8DbContext _context;
        public UserService(Alloc8DbContext context)
        {
            _context = context;
        }
        public async Task<List<Alloc8.ef.Entities.Dashboard.Organisation>> GetAllOrganizations()
        {
            return await _context.organisations.Where(x => x.isDeleted == false).ToListAsync();
        }
        public async Task<PagedResult<UserViewModel>> getAllUsers(int PageNumber = 1, int PageSize = 10, string search = "")
        {
            int TotalCount;
            int TotalPages;
            List<UserViewModel> usersWithRoles = new List<UserViewModel>();
            try
            {
                int ExcludeRecords = (PageSize * PageNumber) - PageSize;
                var query = _context.users
                    .Where(x => x.is_deleted == false)
                    .Where(u => string.IsNullOrEmpty(search) ||
                        EF.Functions.Like(u.Email, "%" + search + "%") ||
                        EF.Functions.Like(u.firstName, "%" + search + "%") ||
                        EF.Functions.Like(u.lastName, "%" + search + "%"));


                usersWithRoles = await query
                .Skip(ExcludeRecords)
                .Take(PageSize)
                .Select(user => new UserViewModel
                {
                    firstName = user.firstName,
                    lastName = user.lastName,
                    email = user.Email,
                    role = user.roleName,
                    active = user.isActive,
                    userId = user.Id,
                    isDeleted = user.is_deleted,
                }).ToListAsync();


                TotalCount = await query.CountAsync();
                TotalPages = (TotalCount % PageSize) == 0 ? Convert.ToInt32(TotalCount / PageSize) : Convert.ToInt32(TotalCount / PageSize) + 1;
            }
            catch (Exception ex)
            {
                throw;
            }
            var result = new PagedResult<UserViewModel>
            {
                data = usersWithRoles,
                totalItems = TotalCount,
                pageNumber = PageNumber,
                pageSize = PageSize,
                totalPages = TotalPages
            };
            return result;

        }

        public async Task<PagedResult<UserViewModel>> getUserForManager(string managerId, int PageNumber = 1, int PageSize = 10, string search = "")
        {
            int TotalCount;
            int TotalPages;
            List<UserViewModel> usersWithRoles = new List<UserViewModel>();
            try
            {
                int ExcludeRecords = (PageSize * PageNumber) - PageSize;
                var query = _context.users
                    .Where(x => x.Id == managerId)
                    .Include(r => r.users)
                    .Where(x => x.is_deleted == false)
                    .Where(u => string.IsNullOrEmpty(search) ||
                        EF.Functions.Like(u.Email, "%" + search + "%") ||
                        EF.Functions.Like(u.firstName, "%" + search + "%") ||
                        EF.Functions.Like(u.lastName, "%" + search + "%"));

                usersWithRoles = await query.SelectMany(r => r.users).Select(um => new UserViewModel
                {
                    firstName = um.user.firstName,
                    lastName = um.user.lastName,
                    email = um.user.Email,
                    role = um.user.roleName,
                    active = um.user.isActive,
                    userId = um.user.Id,
                    isDeleted = um.user.is_deleted,
                }).ToListAsync();

                TotalCount = await query.SelectMany(r => r.users).CountAsync();
                TotalPages = (TotalCount % PageSize) == 0 ? Convert.ToInt32(TotalCount / PageSize) : Convert.ToInt32(TotalCount / PageSize) + 1;
            }
            catch (Exception ex)
            {
                throw;
            }
            var result = new PagedResult<UserViewModel>
            {
                data = usersWithRoles,
                totalItems = TotalCount,
                pageNumber = PageNumber,
                pageSize = PageSize,
                totalPages = TotalPages
            };
            return result;
        }
        public async Task<PagedResult<UserViewModel>> getAllManagers(int PageNumber = 1, int PageSize = 10, string search = "")
        {
            int TotalCount;
            int TotalPages;
            List<UserViewModel> managers = new List<UserViewModel>();
            try
            {

                int ExcludeRecords = (PageSize * PageNumber) - PageSize;

                managers = await _context.users
                .Join(_context.UserRoles,
                      user => user.Id,
                      userRole => userRole.UserId,
                      (user, userRole) => new { user, userRole })
                .Join(_context.Roles,
                      userWithRole => userWithRole.userRole.RoleId,
                      role => role.Id,
                      (userWithRole, role) => new UserViewModel
                      {
                          firstName = userWithRole.user.firstName,
                          lastName = userWithRole.user.lastName,
                          email = userWithRole.user.Email,
                          role = role.Name,
                          active = userWithRole.user.isActive,
                          userId = userWithRole.user.Id,
                          isDeleted = userWithRole.user.is_deleted,
                      })
                .Where(x => x.isDeleted == false && x.role == "Manager") // Filter for managers
                .Where(u => string.IsNullOrEmpty(search) ||
                    EF.Functions.Like(u.email, "%" + search + "%") ||
                    EF.Functions.Like(u.firstName, "%" + search + "%") ||
                    EF.Functions.Like(u.lastName, "%" + search + "%"))
                .Skip(ExcludeRecords)
                .Take(PageSize)
                .ToListAsync();

                TotalCount = await _context.Users
                .Join(_context.UserRoles, user => user.Id, userRole => userRole.UserId, (user, userRole) => new { user, userRole })
                .Where(u => u.userRole.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "Manager").Id) // Count only managers
                .CountAsync();

                TotalPages = (TotalCount % PageSize) == 0 ? TotalCount / PageSize : (TotalCount / PageSize) + 1;
            }
            catch (Exception ex)
            {
                throw;
            }

            var result = new PagedResult<UserViewModel>
            {
                data = managers,
                totalItems = TotalCount,
                pageNumber = PageNumber,
                pageSize = PageSize,
                totalPages = TotalPages
            };

            return result;
        }
    }
}
