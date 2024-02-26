using Alloc8_web.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;
using Alloc8.ef;

namespace Alloc8_web.Services.Admin
{
    public class AdminUserService: IAdminUserService
    {
        private readonly Alloc8DbContext _context;
        public AdminUserService(Alloc8DbContext context)
        {
            _context = context; 
        }
        public async Task<StorageViewModel?> getStorageData()
        {
            try
            {
                var storageViewModel = new StorageViewModel();
                storageViewModel.labels = new List<string> { "Used Space", "Total Space" };
                var data = await _context.databaseLog.FirstOrDefaultAsync();
                if (data == null)
                {
                    return storageViewModel;
                }
                storageViewModel.items = new List<int> { Convert.ToInt32(data.TotalUsedSpaceKB), Convert.ToInt32(data.TotalDatabaseSpaceKB) };
                return storageViewModel;
            }
            catch(Exception ex)
            {

            }
            return null;
          
        }
        public async Task<DataLogsViewModel?> getStorageLogsData()
        {
            DataLogsViewModel viewModel = new DataLogsViewModel();
            viewModel.label = "Database Logs";
            var fetchStorageLogsData = await _context.databaseLog.OrderByDescending(x => x.LogID).ToListAsync();
            if (fetchStorageLogsData == null)
            {
                return viewModel;
            }
            viewModel.minValue = Convert.ToInt32(fetchStorageLogsData.Min(x => x.TotalRecordCount));
            viewModel.maxValue = Convert.ToInt32(fetchStorageLogsData.Max(x => x.TotalRecordCount));
            viewModel.data = fetchStorageLogsData.Select(log => new List<string> { log.LogTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), Convert.ToString(log.TotalRecordCount) }).ToList();
            return viewModel;
        }
    }
}
