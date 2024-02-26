namespace Alloc8_web.ViewModels.User
{
    public class UserViewModel
    {
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? email { get; set; }
        public string? role { get; set; }
        public string? status { get; set; }
        public bool active { get; set; }
        public string? userId { get; set; }
        public bool isDeleted { get; set; }
        public string? profilePicture { get; set; }
        public List<string?>? managers { get; set; }
        public string timezone { set; get; }
    }
}
