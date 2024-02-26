using Alloc8_web.ViewModels.User;

namespace Alloc8_web.Services.User
{
    public interface IAuth
    {
        public bool hasPermission(string permissonName,string permissionType);
        public UserLoginDataViewModel? user();
    }
}
