using Alloc8.ef.Entities.Dashboard;
using Alloc8_web.Utilities;
using Alloc8_web.ViewModels.User;

namespace Alloc8_web.Services.User
{
    public interface IUserService
    {
        public  Task<PagedResult<UserViewModel>> getAllUsers(int PageNumber = 1, int PageSize = 10,string search = "");
        public  Task<PagedResult<UserViewModel>> getUserForManager(string managerId,int PageNumber = 1, int PageSize = 10, string search = "");
        public Task<PagedResult<UserViewModel>> getAllManagers(int PageNumber = 1, int PageSize = 10, string search = "");
        public Task<List<Organisation>> GetAllOrganizations();
    }
}
