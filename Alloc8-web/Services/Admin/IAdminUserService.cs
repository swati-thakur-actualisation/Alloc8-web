using Alloc8_web.ViewModels.Dashboard;

namespace Alloc8_web.Services.Admin
{
    public interface IAdminUserService
    {
        public Task<StorageViewModel?> getStorageData();
        public Task<DataLogsViewModel?> getStorageLogsData();
    }
}
